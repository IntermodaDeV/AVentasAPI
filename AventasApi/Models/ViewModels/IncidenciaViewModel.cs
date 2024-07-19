using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class IncidenciaViewModel
    {
        public int IdAsignacionAsesor { get; set; }
        public string Observacion { get; set; }
        public bool GeneraIncidencia { get; set; }
        public int IdTipoIncidencia { get; set; }
        public List<string> Imagenes { get; set; }
    }

    public class IncidenciaPut
    {
        public int Id { get; set; }
        public int IdEstado { get; set; }
    }

}