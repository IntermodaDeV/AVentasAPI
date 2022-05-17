using AventasApi.Models.ViewModels;
using System;

namespace AventasApi.Models
{
    public class CheckInViewModel
    {
        public int IdAsignacionxAsesor { get; set; }
        public Location location = new Location();
        public DateTime Fecha { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Asesor { get; set; }
        public string observacion { get; set; }
        public string origen { get; set; }
    }
}