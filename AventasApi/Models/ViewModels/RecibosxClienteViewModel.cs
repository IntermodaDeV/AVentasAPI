using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RecibosxClienteViewModel
    {
        public RecibosxClienteViewModel()
        {
            DetalleRecibo = new List<RecibosDetalleViewModel>();
        }
        public int ReciboId { get; set; }
        public string NumeroRecibo { get; set; }
        public string CodigoCliente { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<int> IdTipoPago { get; set; }
        public string Referencia { get; set; }
        public Nullable<System.DateTime> FechaCheque { get; set; }
        public Nullable<int> IdBanco { get; set; }
        public Nullable<int> IdCuentaBancaria { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public string IdMoneda { get; set; }
        public Nullable<bool> Sincronizado { get; set; }
        public string CodigoAsesor { get; set; }
        public Nullable<int> IdFactura { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public List<RecibosDetalleViewModel> DetalleRecibo { get; set; }
    }
}