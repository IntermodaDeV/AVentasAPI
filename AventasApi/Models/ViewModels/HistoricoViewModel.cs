using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class HistoricoViewModel
    {
        public DateTime FechaFactura { get; set; }
        public string Proveedor { get; set; }
        public string NoFactura { get; set; }
        public string Descripcion { get; set; }
        public double ValorFactura { get; set; }
    }
}