using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class AcuerdoVentaApiModel
    {
        public string clasificacion { get; set; }
        public string clienteId { get; set; }
        public string empresaId { get; set; }
        public string monedaId { get; set; }
        public string numero { get; set; }
        public decimal saldo { get; set; }
        public string tipo { get; set; }
        public decimal total { get; set; }
        public string tipoPago { get; set; }
        public decimal entregado { get; set; }
        public decimal facturado { get; set; }
        public decimal liberado { get; set; }
        public List<AcuerdoVentaDetalleApiModel> parmLineasPaymAgreement;

        public AcuerdoVentaApiModel()
        {
            parmLineasPaymAgreement = new List<AcuerdoVentaDetalleApiModel>();
        }
    }
}