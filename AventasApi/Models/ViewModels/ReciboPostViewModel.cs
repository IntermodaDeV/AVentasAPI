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
        public double SaldoFavor { get; set; }
        public List<PagosReciboPostViewModel> Pagos { get; set; }
        public string Descripcion { get; set; }
        public List<int> SubFacturas { get; set; }
        public string CodigoCliente { get; set; }
        public string Tipo { get; set; }
        public string NumPedido { get; set; }
        public string EsContado { get; set; }
        public string NumeroRecibo { get; set; }
        public Location location = new Location();

        public ReciboPostViewModel()
        {
            Pagos = new List<PagosReciboPostViewModel>();
            SubFacturas = new List<int>();
        }
    }
}