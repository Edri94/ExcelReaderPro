﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class bmtktp01Entities : DbContext
    {
        public bmtktp01Entities()
            : base("name=bmtktp01Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<OBSERVACIONES> OBSERVACIONES { get; set; }
        public virtual DbSet<SEGUIMIENTO> SEGUIMIENTO { get; set; }
        public virtual DbSet<SEGUIMIENTO_DOCTOS> SEGUIMIENTO_DOCTOS { get; set; }
        public virtual DbSet<SEGUIMIENTO_OBSERVACIONES> SEGUIMIENTO_OBSERVACIONES { get; set; }
    }
}
