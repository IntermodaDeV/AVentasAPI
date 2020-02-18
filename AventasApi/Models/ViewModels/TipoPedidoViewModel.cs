using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TipoPedidoViewModel
    {
        public int IdTipoPedido { get; set; }
        public string TipoPedido { get; set; }
        public bool HabilitaEstilos { get; set; }
        public string Imagen { get; set; }
        public bool Restrictivo { get; set; }
        public bool Aplica_Todos { get; set; }
    }
}