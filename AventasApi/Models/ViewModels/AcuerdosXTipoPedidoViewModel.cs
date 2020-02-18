using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AcuerdosXTipoPedidoViewModel
    {
        public List<FacturasXAcuerdosViewModel> Acuerdos { get; set; }
        public int IdTipoPedido { get; set; }
        public string TipoPedido { get; set; }
        public bool AgrupaPorCuota { get; set; }

        public AcuerdosXTipoPedidoViewModel()
        {
            this.Acuerdos = new List<FacturasXAcuerdosViewModel>();
        }
    }
}