using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PreguntasViewModel
    {
        public PreguntasViewModel()
        {
            GrupoOpciones = new List<PreguntasOpcionesViewModel>();
        }
        public int Id { get; set; }
        public int SeccionEncuestaId { get; set; }
        public int TipoIngresoId { get; set; }
        public int? GrupoOpcionesId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool RequiereOpciones { get; set; }
        public bool Obligatorio { get; set; }
        public bool RespuestaObligatorio { get; set; }

        public bool Status { get; set; }
        public string Usuario { get; set; }
        public List<int> GrupoOpcionesDetalle { get; set; }
        public List<PreguntasOpcionesViewModel> GrupoOpciones { get; set; }
    }
}