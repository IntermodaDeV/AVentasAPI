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
        public List<DocumentosAplicadosAFacturasViewModel> DocumentosAplicadosxCuotas;
        public DescuentoEnAcuerdosViewModel DescuentoEnAcuerdos;
        public FacturasXAcuerdosViewModel()
        {
            this.Facturas = new List<FacturasXClienteViewModel>();
            this.DocumentosAplicadosxCuotas = new List<DocumentosAplicadosAFacturasViewModel>();

        }
        
        public List<PedidosXClienteViewModel> Pedidos;
    }
}