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
        public int? idColeccion { get; set; }
        public string NombreProducto { get; set; }
        public string GrupoTalla { get; set; }
        public string GrupoImpuesto { get; set; }
        public LineaViewModel Linea { get; set; }
        public List<PrecioXProductoViewModel> Precio { get; set; }
        public decimal? CantidadMinima { get;  set; }
        public List<ColorSinStock> ListaColoresSinStock { get; internal set; }
        public bool StockVisible { get; set; }

        public bool InOut { get; set; }
        public bool Deshabilitado { get; set; }
        public int Prioridad { get; set; }
        public bool Nuevo { get; set; }

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