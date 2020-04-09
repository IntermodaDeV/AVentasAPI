using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ReciboPostViewModel
    {
        public DateTime Fecha { get; set; }
        public DateTime FechaPago { get; set; }
        public List<PagosReciboPostViewModel> Pagos { get; set; }
        public string Descripcion { get; set; }
        public List<int> SubFacturas { get; set; }
        public string CodigoCliente { get; set; }
        public string Tipo { get; set; }

        public ReciboPostViewModel()
        {
            Pagos = new List<PagosReciboPostViewModel>();
            SubFacturas = new List<int>();
        }
    }
}