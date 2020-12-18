using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PantallasFuncionesViewModel
    {
        public int Id { get; set; }

        public int IdPantalla { get; set; }

        public int IdFuncion { get; set; }

        public bool Status { get; set; }

        public string NombrePantalla { get; set; }

        public string Ruta { get; set; }

        public string Usuario { get; set; }

        public bool? ModoOffline { get; set; }
    }
}