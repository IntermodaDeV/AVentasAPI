using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PagosAFacturasXCuotaViewModel
    {
        public int IdPagosAFacturasDeCuota { get; set; }

        public int IdFacturaXCuota { get; set; }

        public string NumeroDocumento { get; set; }

        public Nullable<Decimal> Valor { get; set; }

        public Nullable<DateTime> FechaLiquidacion { get; set; }

        public Nullable<DateTime> FechaDeposito { get; set; }
    }
}