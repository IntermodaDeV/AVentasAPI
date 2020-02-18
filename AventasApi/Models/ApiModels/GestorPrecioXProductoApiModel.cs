using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class GestorPrecioXProductoApiModel
    {
        public string codigo { get; set; }
        public string grupoPrecio { get; set; }
        public string moneda { get; set; }
        public Nullable<decimal> precio { get; set; }
    }
}