using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PagosReciboPostViewModel
    {
        public string  CodigoTipoPago { get; set; }
        public string  TipoPagoDetalle { get; set; }
        public string IdBanco { get; set; }
        public int Orden { get; set; }
        public double Valor { get; set; }
        public string IdMoneda { get; set; }
        public string Referencia { get; set; }
        public string ReferenciaTransaccionAbierta { get; set; }

    }
}