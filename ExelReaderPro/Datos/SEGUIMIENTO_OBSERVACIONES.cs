//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExelReaderPro.Datos
{
    using System;
    using System.Collections.Generic;
    
    public partial class SEGUIMIENTO_OBSERVACIONES
    {
        public int Num_Solicitud { get; set; }
        public int Id_Observacion { get; set; }
        public int ID { get; set; }
    
        public virtual OBSERVACIONES OBSERVACIONES { get; set; }
        public virtual SEGUIMIENTO SEGUIMIENTO { get; set; }
    }
}
