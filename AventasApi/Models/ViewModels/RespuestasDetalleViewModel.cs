using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RespuestasDetalleViewModel
    {
        public int Id { get; set; }
        public int RespuestaId { get; set; }
        public int PreguntaId { get; set; }
        public int? PreguntasOpcionesId { get; set; }
        public List<string> PreguntasOpciones { get; set; }
        public string RespuestaAlfanumerica { get; set; }
        public int? RespuestaNumerica { get; set; }
        public string Usuario { get; set; }
    }
}