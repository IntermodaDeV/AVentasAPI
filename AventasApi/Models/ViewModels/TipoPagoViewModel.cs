using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TipoPagoViewModel
    {
        public int IdTipoPago { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public string EmpresaId { get; set; }
        public virtual ICollection<TipoPagoDetalleViewModel> TiposdePagoDetalle { get; set; }
        public TipoPagoViewModel()
        {
            this.TiposdePagoDetalle = new HashSet<TipoPagoDetalleViewModel>();
        }
    }
}