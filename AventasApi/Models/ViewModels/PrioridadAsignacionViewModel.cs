using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PrioridadAsignacionViewModel
    {
        public int idPrioridad { get; set; }
        public string NombrePrioridad { get; set; }
        public Nullable<bool> Estatus { get; set; }
        public string ColorBorde { get; set; }
        public string ColorRelleno { get; set; }
    }
}