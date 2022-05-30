using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class FacturasEnCuotasAcuerdoViewModel
    {
        public int IdFacturaXCuota { get; set; }

        public int IdCuotaXAcuerdo { get; set; }

        public string Factura { get; set; }

        public Decimal Valor { get; set; }

        public Nullable<DateTime> FechaFactura { get; set; }

        public Nullable<DateTime> FechaVencimiento { get; set; }

        public List<PagosAFacturasXCuotaViewModel> PagosEnFacturas { get; set; }

        public FacturasEnCuotasAcuerdoViewModel()
        {
            PagosEnFacturas = new List<PagosAFacturasXCuotaViewModel>();
        }
    }
}