using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models
{
    public class ProductoXColeccionApiModel
    {
        public string productoId { get; set; }
        public string codigoColeccion { get; set; }
        public string nombreProducto { get; set; }
        public string grupoTallaId { get; set; }
        public string linea { get; set; }
        public int backorder  { get; set; }
        public int multiplo { get; set; }
        //public Nullable<decimal> fisicaDisponible { get; set; }
        public List<ImagenXProductoApiModel> listaImagenes;
        public List<EdadApiModel> edad;
        public string GrupoImpuesto { get; set; }



        public ProductoXColeccionApiModel()
        {
            this.listaImagenes = new List<ImagenXProductoApiModel>();
            this.edad = new List<EdadApiModel>();
        }
    }

    public class EdadApiModel
    {
        public string codigo { get; set; }
        public string description { get; set; }
        public string description2 { get; set; }
    }
}