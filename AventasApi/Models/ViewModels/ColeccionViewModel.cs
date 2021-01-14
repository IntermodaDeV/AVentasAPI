using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ColeccionViewModel
    {
        public int IdColeccion { get; set; }
        public string CodigoColeccion { get; set; }
        public string EmpresaId { get; set; }
        public string Nombre { get; set; }
        public string ColeccionTipo { get; set; }
        public string FotoPortada { get; set; }
        public System.DateTime? DisenoInicio { get; set; }
        public System.DateTime? DisenoFinal { get; set; }
        public System.DateTime? VentaInicio { get; set; }
        public System.DateTime? VentaFinal { get; set; }
        public System.DateTime? ProduccionInicio { get; set; }
        public System.DateTime? ProduccionFinal { get; set; }
        public System.DateTime? EntregaInicio { get; set; }
        public System.DateTime? EntregaFinal { get; set; }
        public int? Estatus { get; set; }
        public string GrupoPrecio { get; internal set; }

        public List<AtributosViewModel> AtributosXColeccion;
        public List<EdadesViewModel> Edades;
        public List<string> Lineas;

        public ColeccionViewModel()
        {
            this.Edades = new List<EdadesViewModel>();
            this.AtributosXColeccion = new List<AtributosViewModel>();
        }
    }
}