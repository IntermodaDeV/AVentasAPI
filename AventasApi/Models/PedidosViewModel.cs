using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class PedidosViewModel
    {
        public string  pedidoId { get; set; }
        public string  empresaId { get; set; }
        public string  cliente { get; set; }
        public string  detallePedido { get; set; }
        public string  observacion { get; set; }
        public DateTime  fechaEntrega { get; set; }
        public DateTime  fecha { get; set; }
        public string  acuerdoVentaId { get; set; }
    }
}