using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class SeccionEncuestaViewModel
    {
        public int Id { get; set; }
        public int EncuestaId { get; set; }
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Obligatorio { get; set; }
        public bool Status { get; set; }
        public string Usuario { get; set; }
    }
}