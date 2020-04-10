using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models
{
    public class ColeccionApiModel
    {
        public string codigoColeccion { get; set; }
        public string empresaId { get; set; }
        public string nombre { get; set; }
        public string linea { get; set; }//
        public string coleccionTipo { get; set; }
        public System.DateTime? disenoInicio { get; set; }
        public System.DateTime? disenoFinal { get; set; }
        public System.DateTime? ventaInicio { get; set; }
        public System.DateTime? ventaFinal { get; set; }
        public System.DateTime? produccionInicio { get; set; }
        public System.DateTime? produccionFinal { get; set; }
        public System.DateTime? entregaInicio { get; set; }
        public System.DateTime? entregaFinal { get; set; }
        public int estatus { get; set; }

        //public List<ProductoXColeccionApiModel> productos;
        public List<LineaApiModel> listaLineas;

        public ColeccionApiModel()
        {
            this.listaLineas = new List<LineaApiModel>();
        }
    }
}