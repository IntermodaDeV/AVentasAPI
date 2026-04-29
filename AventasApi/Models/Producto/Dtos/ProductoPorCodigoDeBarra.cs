using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.Producto
{
    public class ProductoPorCodigoDeBarraDto
    {
        public string productoId { get; set; }
        public string colorId { get; set; }
        public string tallaId { get; set; }
        public string colorname { get; set; }
        public string productline { get; set; }
        public string edad { get; set; }
        public string descripcionedad { get; set; }
        public string sublinea { get; set; }
        public string nombre { get; set; }
    }
}