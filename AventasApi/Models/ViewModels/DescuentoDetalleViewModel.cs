using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DescuentoDetalleViewModel
    {
        public string Linea { get; set; }

        public string CodigoDescuento { get; set; }

        public int? DiasDescuento { get; set; }

        public Decimal?  Porcentaje { get; set; }
    }
}