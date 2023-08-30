using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CuotasViewModel
    {
        public int IdSubFactura { get; set; }
        public string Factura { get; set; }
        public string NumeroFEL { get; set; }
        public string CodigoCliente { get; set; }
        public string EmpresaId { get; set; }
        public string IdMoneda { get; set; }
        public string IdAcuerdoxCliente { get; set; }
        public Nullable<System.DateTime> FechaFactura { get; set; }
        public Nullable<System.DateTime> FechaVencimiento { get; set; }
        public Nullable<System.DateTime> FechaMaxDescuento { get; set; }
        public Nullable<System.DateTime> FechaVencimientoDescuento { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<decimal> SaldoDivisa { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public Nullable<decimal> PendientePago { get; set; }
        public string Referencia { get; set; }
        public string TipoDocumento { get; set; }
        public string ReferenciaFacturas { get; set; }
        public string ReferenciaAcuerdo { get; set; }
        public Nullable<int> NumeroCuota { get; set; }
        public Nullable<decimal> ValorCuota { get; set; }
        public Nullable<decimal> ValorVencidoCuota { get; set; }
        public string ReferenciaCuotas { get; set; }
        public Nullable<int> IdFactura { get; set; }
        public Nullable<bool> completaCuota { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public Nullable<decimal> Flete { get; set; }
        public bool ExcepcionDescuento { get; set; }
        public decimal SaldoCuota { get; set; }
        public Nullable<decimal> DisponibleCuota { get; set; }
    }
}