using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AcuerdoVentaDetalleViewModel
    {
        public int IdAcuerdoxClienteDetalle { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<decimal> Monto { get; set; }
        public Nullable<decimal> Saldo { get; set; }
    }
}