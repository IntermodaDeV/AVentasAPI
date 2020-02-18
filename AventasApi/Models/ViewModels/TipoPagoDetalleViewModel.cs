using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TipoPagoDetalleViewModel
    {
        public int IdTipoPagoDetalle { get; set; }
        public string Codigo { get; set; }
        public string CodigoDetalle { get; set; }
        public string Descripcion { get; set; }
        public string EmpresaId { get; set; }
        public Nullable<int> IdTipoPago { get; set; }
    }
}