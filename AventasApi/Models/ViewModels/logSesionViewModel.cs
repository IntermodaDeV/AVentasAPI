using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class logSesionViewModel
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string version_navegador { get; set; }
        public string Ip_publica { get; set; }
        public string latitud { get; set; }
        public string longitud { get; set; }
        public string version_App { get; set; }
        public DateTime Fecha { get; set; }
    }
}