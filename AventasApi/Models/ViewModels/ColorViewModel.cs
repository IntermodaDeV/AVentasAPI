using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ColorViewModel
    {
        public int IdColorxProducto { get; set; }
        public string CodigoColor { get; set; }
        public string Color { get; set; }
        public string NombreColor { get; set; }
        public int Prioridad { get; set; }
        public bool Deshabilitado { get; set; }
        public List<FotografiasXProductoViewModel> ListaImagenes;
        public ColorViewModel()
        {
            ListaImagenes = new List<FotografiasXProductoViewModel>();
        }
    }
}