using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ProductoXColeccionViewModel
    {
        public string ProductoId { get; set; }
        public int CodigoProducto { get; set; }
        public string CodigoColeccion { get; set; }
        public string NombreProducto { get; set; }
        public string GrupoTalla { get; set; }
        public LineaViewModel Linea { get; set; }
        public List<PrecioXProductoViewModel> Precio { get; set; }
        public List<TallaViewModel> ListaTalla;
        public List<ColorViewModel> ListaColores;
        public List<FisicoDisponibleViewModel> fisicaDisponible;
        public List<FotografiasXProductoViewModel> ListaImagenes;
        public List<AtributosViewModel> AtributosXProducto;
        
        public ProductoXColeccionViewModel () {
            this.ListaTalla = new List<TallaViewModel> ();
            this.ListaColores = new List<ColorViewModel> ();
            this.fisicaDisponible = new List<FisicoDisponibleViewModel> (); 
            this.ListaImagenes = new List<FotografiasXProductoViewModel> ();
            this.AtributosXProducto = new List<AtributosViewModel> ();
            this.Precio = new List<PrecioXProductoViewModel>();
        }
    }
}