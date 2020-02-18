using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class TallasXColorViewModel
    {
        public int IdRegistro { get; set; }
        public int? CodigoColor { get; set; }
        public int? IdTalla { get; set; }
        public string Talla { get; set; }

    }
}