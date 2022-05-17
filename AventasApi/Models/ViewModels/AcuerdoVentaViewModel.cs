using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AcuerdoVentaViewModel
    {
        public string IdAcuerdoxCliente { get; set; }
        public string CodigoCliente { get; set; }
        public Nullable<int> IdTipoPedido { get; set; }
        public string IdMoneda { get; set; }
        public string EmpresaId { get; set; }
        public string Tipo { get; set; }
        public string TipoPago { get; set; }
        public string Linea { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<decimal> Liberado { get; set; }
        public Nullable<decimal> Facturado { get; set; }
        public Nullable<decimal> Entregado { get; set; }
        public Nullable<DateTime> Desde { get; set; }
        public Nullable<DateTime> Hasta { get; set; }
        public List<AcuerdoVentaDetalleViewModel> detalleAcuerdo  { get; set; }
        public List<CuotasDeAcuerdoViewModel> CuotasDeAcuerdo { get; set; }

        public AcuerdoVentaViewModel()
        {
            detalleAcuerdo = new List<AcuerdoVentaDetalleViewModel>();
            CuotasDeAcuerdo = new List<CuotasDeAcuerdoViewModel>();
        }
    }
}