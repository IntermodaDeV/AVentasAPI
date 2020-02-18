using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class RazonNoVentaCausaViewModel
    {
        public int IdRazonNoVentaCausa { get; set; }
        public Nullable<int> IdRazonNoVentaTipo { get; set; }
        public string Causa { get; set; }
    }
}