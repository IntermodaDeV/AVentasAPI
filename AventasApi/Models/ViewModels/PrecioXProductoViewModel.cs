using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PrecioXProductoViewModel
    {
        public string GrupoPrecio { get; set; }
        public string IdMoneda { get; set; }
        public Nullable<decimal> Precio { get; set; }
    }
}