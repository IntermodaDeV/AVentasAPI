using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class GrupoOpcionesDetalleViewModel
    {
        public int Id { get; set; }
        public int GrupoOpcionesId { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
    }
}