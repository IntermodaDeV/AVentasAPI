using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class ReciboApiModel
    {
        public string COMPANY { get; set; }
        public string ASESOR { get; set; }
        public string ASESOR_NOMBRE { get; set; }
        public string ASESOR_DIARIO { get; set; }
        public string RECIBO { get; set; }
        public string CLIENTE { get; set; }
        public string MONEDA { get; set; }
        public string FECHA { get; set; }
        public string DESCRIPCION { get; set; }
        public string TOTAL_RECIBO { get; set; }
        public string TOTAL_FACTURAS { get; set; }
        public string TOTAL_APLICADO { get; set; }
        public string TIPO_PAGO { get; set; }
        public string SPEC_PAGO { get; set; }
        public string BANCO { get; set; }
        public string FECHA_PAGO { get; set; }
        public string REFERENCIA { get; set; }
        public string FACTURA { get; set; }
        public string APLICADO { get; set; }
        public string DESCUENTO { get; set; }
        public string REF_TRANSOPEN { get; set; }
    }
}