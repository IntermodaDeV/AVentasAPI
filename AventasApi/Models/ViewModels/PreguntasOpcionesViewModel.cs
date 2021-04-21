using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PreguntasOpcionesViewModel
    {
        public int Id { get; set; }
        public int PreguntaId { get; set; }
        public int GrupoOpcionesDetalleId { get; set; }
        public string GrupoOpcionesDetalle { get; set; }
        public bool Status { get; set; }
        public string Usuario { get; set; }
    }
}