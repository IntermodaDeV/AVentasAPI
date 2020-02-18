using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class AcuerdoVentaDetalleApiModel
    {
        public System.DateTime? fecha { get; set; }
        public Decimal monto { get; set; }
        public Decimal saldo { get; set; }

    }
}