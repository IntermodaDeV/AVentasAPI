using System;

namespace AventasApi.Models
{
    public class FacturaPendiente
    {
        public string Tipo { get; set; }
        public decimal? Valor { get; set; }
        public string Moneda { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public string CodigoCliente { get; set; }
        public string Factura { get; set; }
        public string NumeroDocumento { get; set; }
        public string Estado { get; set; }
        public string CreadoPor { get; set; }
        public int? ReferenciaAx { get; set; }
        public string IdentificadorAx { get; set; }
        public string NumeroFel { get; set; }
    }
}