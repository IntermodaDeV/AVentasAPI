using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DetallePedidoPostViewModel
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoColor { get; set; }
        public string Cantidad { get; set; }
        public string Unidad { get; set; }
        public string PrecioUnitario { get; set; }
        public string Talla { get; set; }
        public string CodigoColeccion { get; set; }
        public string PorcentajeDescuento { get; set; }
    }
}