using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RecibosDetalleViewModel
    {
        public int IdReciboDetalle { get; set; }
        public Nullable<int> ReciboId { get; set; }
        public Nullable<int> IdSubFactura { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public Nullable<decimal> ValorSinDescuento { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public int DiasVencimiento { get; set; }
        public string Factura { get; set; }
        public string Tipo { get; set; }
    }
}