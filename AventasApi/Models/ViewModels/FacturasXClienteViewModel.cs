using System;
using System.Collections.Generic;

namespace AventasApi.Models.ViewModels
{
    public class FacturasXClienteViewModel
    {
        public FacturasXClienteViewModel()
        {
            this.Cuotas = new List<CuotasViewModel>();
            this.DocumentosAplicadosAFacturas = new List<DocumentosAplicadosAFacturasViewModel>();
        }
        public int IdFactura { get; set; }
        public string Factura { get; set; }
        public string NumeroFEL { get; set; }
        public string CodigoCliente { get; set; }
        public string EmpresaId { get; set; }
        public string IdMoneda { get; set; }
        public string Tipo { get; set; }
        public Nullable<System.DateTime> FechaFactura { get; set; }
        public Nullable<System.DateTime> FechaVencimiento { get; set; }
        public Nullable<System.DateTime> FechaMaxDescuento { get; set; }
        public Nullable<decimal> TotalFactura { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<decimal> PendienteFactura { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public string FacturaStatus { get; set; }
        public Nullable<int> NumeroPagos { get; set; }
        public string Referencia { get; set; }
        public string IdLinea { get; set; }
        public string LineaString { get; set; }
        public string TipoPedidoString { get; set; }
        public Nullable<int> IdTipoPedido { get; set; }
        public List<CuotasViewModel> Cuotas { get; set; }
        public List<DocumentosAplicadosAFacturasViewModel> DocumentosAplicadosAFacturas { get; set; }
        public bool ExcepcionDescuento { get; set; }
        public int DiasGracia { get; set; }
        public string CodigoDescuento { get; set; }

    }
}