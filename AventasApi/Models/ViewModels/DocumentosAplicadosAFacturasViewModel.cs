using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DocumentosAplicadosAFacturasViewModel
    {
        public int Id { get; set; }
        public string Factura { get; set; }
        public string Voucher { get; set; }
        public string TipoDocumento { get; set; }
        public string FacturaDocumento { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public Nullable<decimal> MontoPorAplicar { get; set; }
        public string CodigoCliente { get; set; }
        public string SecuenciaNumerica { get; set; }
        public string Moneda { get; set; }
        public string Empresa { get; set; }
    }
}