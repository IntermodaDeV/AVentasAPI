using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class SubFacturasXTipoPedidoViewModel
    {
        public int IdTipoPedido { get; set; }
        public string TipoPedido { get; set; }
        public List<FacturasXClienteViewModel> Facturas;

        public SubFacturasXTipoPedidoViewModel()
        {
            Facturas = new List<FacturasXClienteViewModel>();
        }
    }
}