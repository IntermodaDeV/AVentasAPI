using System;

namespace AventasApi.Models
{
    public class EncuestaCompletada
    {
        public int RespuestaId { get; set; }
        public int EncuestaId { get; set; }
        public string Encuesta { get; set; }
        public string Cliente { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
    }
}