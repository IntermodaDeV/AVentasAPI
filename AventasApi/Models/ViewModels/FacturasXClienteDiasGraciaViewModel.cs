using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AventasApi.Models.ViewModels
{
    public class FacturasXClienteDiasGraciaViewModel
    {
        public string Tipo { get; set; }
        public string Factura { get; set; }
        public string CodigoCliente { get; set; }
        public DateTime? FechaFactura { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaMaxDescuento { get; set; }
        public decimal? TotalFactura { get; set; }
        public decimal? Saldo { get; set; }
        public decimal? Descuento { get; set; }
        public int DiasGracia { get; set; }

    }
}