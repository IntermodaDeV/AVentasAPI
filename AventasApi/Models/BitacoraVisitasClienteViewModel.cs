using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class BitacoraVisitasClienteViewModel
    {
        public int IdBitacoraVisitaCliente { get; set; }
        public int? IdAsignacionxAsesor { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<int> IdRazonNoVentaTipo { get; set; }
        public Nullable<int> IdRazonNoVentaCausa { get; set; }
        public string CodigoCliente { get; set; }
        public string CodigoAsesor { get; set; }
        public string Observacion { get; set; }
       
    }
}