using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ColorViewModel
    {
        public string CodigoColor { get; set; }
        public string Color { get; set; }
        public string NombreColor { get; set; }
        public List<FotografiasXProductoViewModel> ListaImagenes;
        public ColorViewModel()
        {
            ListaImagenes = new List<FotografiasXProductoViewModel>();
        }
    }
}