using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExelReaderPro.Datos;
using ExelReaderPro.Model;
using ExelReaderPro.Negocio;
using IronXL;

namespace ExelReaderPro
{
    public partial class Form1 : Form
    {
        List<string> nombres_archivos;
        WorkSheet hoja;
        List<ObservacionSeguimiento> lista_observaciones;

        public Form1()
        {
            InitializeComponent();

            nombres_archivos = new List<string>();
            lista_observaciones = new List<ObservacionSeguimiento>();

            MessageBox.Show("Indica una ruta odnde se guardara el Log");
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                Log.RutaLog = folderBrowserDialog1.SelectedPath + "\\";
                Log.EscribeLog = true;
            }

            Log.Escribe("Establecida Ruta Log");
            this.Show();
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog1.Multiselect = true;
                          
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (!backgroundWorker1.IsBusy)
                    {
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Ya hay una tarea ejecutandose. Favor de Esperar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }                     
                }            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
        }

        private int EncontrarUltimaFila(WorkSheet hoja)
        {
            try
            {
                for (int i = 0; i < hoja.RowCount; i++)
                {
                    if (hoja.Rows[i].Columns[0].ToString().Contains("Total") == true)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private List<ObservacionSeguimiento> GetObservaciones(string observaciones, int num_seguimiento)
        {
            try
            {
                char[] delims = new[] { '\r', '\n' };
                string[] strings = observaciones.Split(delims, StringSplitOptions.RemoveEmptyEntries);

                List<ObservacionSeguimiento> resultado = new List<ObservacionSeguimiento>();

                foreach (var item in strings)
                {
                    resultado.Add(new ObservacionSeguimiento { Observacion = item, Num_Seguimiento = num_seguimiento  });
                }

                return resultado;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<ObservacionSeguimiento>();
            }
        }

        private string GetFileName(string fileName)
        {
            try
            {
                string[] partes = fileName.Split('\\');

                return partes[partes.Length - 1];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "";
            }
        }

        private string GetExtension(string fileName)
        {
            try
            {
                string[] partes = fileName.Split('.');
                int divisiones = partes.Length;

                return partes[divisiones - 1];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void LecturaArchivo()
        {
            
        }

        private void btnCargarBd_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker2.IsBusy)
            {
                backgroundWorker2.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Ya hay una tarea ejecutandose. Favor de Esperar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
                      
        }

        private bool CargarObservacion(ObservacionSeguimiento observacion, bmtktp01Entities context, DbContextTransaction trx)
        {
                        
            string fecha_busqueda = observacion.Observacion.Substring(0, 26);
            Log.Escribe($"1) Encontrar observacion: {Environment.NewLine} {observacion.Observacion}");
            OBSERVACIONES observacion_encontrada = context.OBSERVACIONES.Where(x => x.Observaciones1.Contains(fecha_busqueda)).FirstOrDefault();

            if (observacion_encontrada != null)
            {
                Log.Escribe($"2) Observacion encontrada.");

                SEGUIMIENTO_OBSERVACIONES seg_obs = context.SEGUIMIENTO_OBSERVACIONES.Where(x => x.Num_Solicitud == observacion.Num_Seguimiento && x.Id_Observacion == observacion_encontrada.Id_Observacion).FirstOrDefault();

                if (seg_obs == null)
                {
                    Log.Escribe($"3) Observacion {observacion_encontrada.Id_Observacion} no  dada de alta en SEGUIMIENTO_OBSERVACIONES");

                    context.SEGUIMIENTO_OBSERVACIONES.Add(new SEGUIMIENTO_OBSERVACIONES { Id_Observacion = observacion_encontrada.Id_Observacion, Num_Solicitud = observacion.Num_Seguimiento });

                    int insertados = context.SaveChanges();

                    if(insertados > 0)
                    {
                        Log.Escribe($"4) Observacion {observacion_encontrada.Id_Observacion} insertada");
                        return true;
                    }
                    else 
                    {
                        Log.Escribe($"4) Observacion {observacion_encontrada.Id_Observacion} NO insertada");
                        return false;
                    } 

                }
                else
                {
                    Log.Escribe($"3) Observacion {observacion_encontrada.Id_Observacion} ya se ha dado de alta  anteriormente en SEGUIMIENTO_OBSERVACIONES");
                    return false;
                }
            }
            else
            {
                Log.Escribe($"2) Observacion del dia {fecha_busqueda} no encontrada");

                return false;
            }                                        
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Invoke(new MethodInvoker(delegate {

                    nombres_archivos = openFileDialog1.FileNames.ToList();

                    lstArchivos.DataSource = nombres_archivos;
                    lstArchivos.Refresh();                 
                }));


                foreach (string FileName in nombres_archivos)
                {                  
                    txtArchivo.Invoke(new MethodInvoker(delegate {
                        txtArchivo.Text = FileName;
                    }));

                    string extension = GetExtension(FileName);

                    string nombre_archivo = GetFileName(FileName);

                    Log.Escribe("Archivo: ***** " + nombre_archivo + " *****");

                    WorkBook libro = WorkBook.Load(FileName);
                    hoja = libro.WorkSheets.First();

                    int filas = hoja.RowCount;
                    int columnas = hoja.ColumnCount;

                    int ultima_fila = EncontrarUltimaFila(hoja);
                    int filas_datos = 0;

                    Log.Escribe(" - El archivo tiene " + filas + " filas.");

                    SEGUIMIENTO seguimiento;

                    lblArchivoCargando.Invoke(new MethodInvoker(delegate {
                        lblArchivoCargando.Text = GetFileName(FileName);
                    }));

                    

                    for (int i = 0; i <= filas - 1; i++)
                    {
                        int porcentaje = (i * 100) / filas;
                        backgroundWorker1.ReportProgress(porcentaje);

                        if (i >= 2)
                        {
                            if (hoja.Rows[i].Columns[1].ToString().Trim() != "")
                            {
                                seguimiento = new SEGUIMIENTO
                                {
                                    Num_Solicitud = Int32.Parse(hoja.Rows[i].Columns[1].ToString()),
                                };

                                lblNumSolicitud.Invoke(new MethodInvoker(delegate {
                                    lblNumSolicitud.Text = seguimiento.Num_Solicitud.ToString();
                                }));

                                

                                string observaciones = hoja.Rows[i].Columns[41].ToString();

                                lista_observaciones.AddRange(GetObservaciones(observaciones, seguimiento.Num_Solicitud));

                                if (lista_observaciones == null)
                                {
                                    MessageBox.Show("Fallo al obtener las observaciones");
                                }
                            }
                            else
                            {
                                break;
                            }
                            filas_datos = i;

                            Log.Escribe("- Filas encontradas con datos: " + filas_datos);
                        }
                    }
                }

                dataGridView1.Invoke(new MethodInvoker(delegate {
                    dataGridView1.DataSource = lista_observaciones;
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Refresh();
                }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            lblArchivoCargando.Text = "TERMINADO";
            lblNumSolicitud.Text = "0";
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            MessageBox.Show(" Se insertaran " + lista_observaciones.Count + " observaciones nuevas.");

            using (bmtktp01Entities context = new bmtktp01Entities())
            {
                using (DbContextTransaction trx = context.Database.BeginTransaction())
                {
                    try
                    {
                        int progreso = 0;

                        foreach (ObservacionSeguimiento observacion in lista_observaciones)
                        {
                            CargarObservacion(observacion, context, trx);
                            progreso ++;

                            int porcentaje = (progreso * 100) / lista_observaciones.Count;
                            backgroundWorker2.ReportProgress(porcentaje);

                            lblArchivoCargando.Invoke(new MethodInvoker(delegate {
                                lblArchivoCargando.Text = "Cargando observacion " + observacion.Observacion;
                            }));

                        }
                        trx.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                        trx.Rollback();
                    }
                   

                }
            }        
            
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            lblArchivoCargando.Text = "OBSERVACIONES CARGADAS";
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
    }
}
