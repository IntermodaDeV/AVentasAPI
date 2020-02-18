using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AsignacionXAsesorViewModel
    {
        public int IdAsignacionxAsesor { get; set; }
        public DateTime? HoraInicio { get; set; }
        public DateTime? HoraFin { get; set; }
        public string cliente { get; set; }
        public Nullable<int> IdPrioridad { get; set; }
        public Nullable<int> IdTipoVisita { get; set; }
        public string Observacion { get; set; }
       public string ColorRelleno { get; set; }
    }
}