using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class HistorialViewModel
    {
        public int IdGastoViajeDetalle { get; set; }
        public string tipo { get; set; }
        public string categoria { get; set; }
        public double ValorFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; }
    }
}