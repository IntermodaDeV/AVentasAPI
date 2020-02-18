using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AccionesViewModel
    {
        public int IdAccion { get; set; }
        public string Accion { get; set; }
        public string UrlRedirect { get; set; }
        public Nullable<bool> Estado { get; set; }
        public Nullable<int> Orden { get; set; }
    }
}