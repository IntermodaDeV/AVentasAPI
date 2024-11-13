using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ServicioViewModel
    {
        public int Id { get; set; }
        public DateTime ProximaEjecucionEnvio { get; set; }
        public DateTime ProximaEjecucionGenerarArchivo { get; set; }
        public bool ReIniciar { get; set; }
        public bool ReIniciarGenerarcionArchivo { get; set; }
    }
}