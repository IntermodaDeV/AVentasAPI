using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PrecioEspecificoViewModel
    {
        public int IdPrecioEspecifico { get; set; }
        public string IdMoneda { get; set; }
        public Nullable<int> IdProducto { get; set; }
        public string GrupoPrecio { get; set; }
        public Nullable<int> IdFisicoDisponible { get; set; }
        public Nullable<decimal> Precio { get; set; }
    }
}