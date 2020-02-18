using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class LineaViewModel
    {
        public string IdLinea { get; set; }
        public string Linea { get; set; }
        public string Imagen { get; set; }
        public List<ColeccionViewModel> colecciones = new List<ColeccionViewModel>();
    }
}