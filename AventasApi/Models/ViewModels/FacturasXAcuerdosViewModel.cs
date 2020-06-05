using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class FacturasXAcuerdosViewModel
    {
        public string Acuerdo { get; set; }
        public string Valor { get; set; }
        public string Disponible { get; set; }
        //public string SaldoTotal { get; set; }

        public List<FacturasXClienteViewModel> Facturas;
        public FacturasXAcuerdosViewModel()
        {
            this.Facturas = new List<FacturasXClienteViewModel>();

        }

        public List<PedidosXClienteViewModel> Pedidos;
    }
}