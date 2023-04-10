using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExelReaderPro.Datos;
using ExelReaderPro.Model;
using IronXL;

namespace ExelReaderPro
{
    public partial class Form1 : Form
    {
        List<string> nombres_archivo;
        WorkSheet hoja;
        List<ObservacionSeguimiento> lista_observaciones;

        public Form1()
        {
            InitializeComponent();

            nombres_archivo = new List<string>();
            lista_observaciones = new List<ObservacionSeguimiento>();
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtArchivo.Text = openFileDialog1.FileName;

                    string extension = GetExtension(openFileDialog1.FileName);

                    string nombre_archivo = GetFileName(openFileDialog1.FileName); ;

                    nombres_archivo.Add(nombre_archivo);

                    WorkBook libro = WorkBook.Load(openFileDialog1.FileName);
                    hoja = libro.WorkSheets.First();

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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int filas = hoja.RowCount;
            int columnas = hoja.ColumnCount;

            int ultima_fila = EncontrarUltimaFila(hoja);

            SEGUIMIENTO seguimiento;

            for (int i = 0; i <= filas - 1; i++)
            {
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
                }
            }

            dataGridView1.Invoke(new MethodInvoker(delegate {
                
                dataGridView1.DataSource = lista_observaciones;
                dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Refresh();

            }));

            lstArchivos.DataSource = nombres_archivo;
        
        }
    }
}
