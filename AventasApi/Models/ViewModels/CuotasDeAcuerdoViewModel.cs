using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CuotasDeAcuerdoViewModel
    {
        public int IdCuotasXAcuerdoVenta { get; set; }

        public string IdAcuerdoVenta { get; set; }

        public int NumCuota { get; set; }

        public Decimal ValorCuota { get; set; }

        public Nullable<Decimal> SaldoDisponible { get; set; }

        public Nullable<DateTime> FechaVencimiento { get; set; }

        public List<FacturasEnCuotasAcuerdoViewModel> FacturasCuotas { get; set; }

        public CuotasDeAcuerdoViewModel()
        {
            FacturasCuotas = new List<FacturasEnCuotasAcuerdoViewModel>();
        }
    }
}