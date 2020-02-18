using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.CustomerLocationApp
{
    public class CoordenadasXClienteViewModel
    {
        public int IdCoordenadasXcliente { get; set; }
        public string CodigoCliente { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? Precision { get; set; }
        public DateTime? Fecha { get; set; }
    }
}