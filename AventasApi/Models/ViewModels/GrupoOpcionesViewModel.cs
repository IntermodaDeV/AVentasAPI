using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class GrupoOpcionesViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Status { get; set; }
        public string Usuario { get; set; }
    }
}