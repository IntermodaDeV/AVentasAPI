using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AventasApi.Models.ApiModels;

namespace AventasApi.Models.ViewModels
{
    public class PedidosFallidosViewModel
    {
        public PedidoApiModel Pedido{ get; set; }
        public DateTime? Fecha{ get; set; }
        public string MensajeAx{ get; set; }
    }
}