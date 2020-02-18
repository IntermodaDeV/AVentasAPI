using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DetPedTempViewModel
    {
        //public string PedidoId { get; set; }
        public string CodigoCliente { get; set; }
        public string ProductoId { get; set; }
        public string CodigoColor { get; set; }
        public string Talla { get; set; }
        public decimal Cantidad { get; set; }
        public decimal MontoLinea { get; set; }
        public decimal PrecioUnitario { get; set; }

    }
}