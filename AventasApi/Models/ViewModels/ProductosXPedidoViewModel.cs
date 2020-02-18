using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ProductosXPedidoViewModel
    {
        public string ProductoId { get; set; }
        public string CodigoColeccion { get; set; }
        public List<PrecioXProductoViewModel> Precio { get; set; }

        public LineaViewModel Linea = new LineaViewModel();

        public List<TallaViewModel> ListaTalla;
        public List<ColorViewModel> ListaColores;
        public List<List<int?>> matriz;
        
    }
}