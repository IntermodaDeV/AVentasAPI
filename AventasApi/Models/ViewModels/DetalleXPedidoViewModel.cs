using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DetalleXPedidoViewModel
    {
        public int IdRegistro { get; set; }
        public string PedidoId { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public Nullable<decimal> Cantidad { get; set; }
        public string IdColor { get; set; }
        public string NombreColor { get; set; }
        public Nullable<int> Linea { get; set; }
        public Nullable<decimal> MontoLinea { get; set; }
        public Nullable<decimal> PrecioUnitario { get; set; }
        public string Talla { get; set; }
    }
}