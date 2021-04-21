using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class BodegaAlmacenesViewModel
    {
        public int AlmacenId { get; set; }
        public string Almacen { get; set; }
        public string Nombre { get; set; }
        public int? SitioId { get; set; }
        public string CodigoSitio { get; set; }
        public string EmpresaId { get; set; }
        public string Etiqueta { get; set; }
        public bool Estatus { get; set; }
        public bool BodegaPrincipal { get; set; }
    }
}