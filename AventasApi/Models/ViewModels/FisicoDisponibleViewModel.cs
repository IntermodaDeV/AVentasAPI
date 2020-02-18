using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class FisicoDisponibleViewModel
    {
        public string CodigoColor { get; set; }
        public string IdTalla { get; set; }
        public decimal? Cantidad { get; set; }
        public Nullable<decimal> MinStock { get; set; }
        public List<PrecioEspecificoViewModel> PreciosEspecificos { get; set; }
        public FisicoDisponibleViewModel()
        {
            PreciosEspecificos = new List<PrecioEspecificoViewModel>();
        }
    }
}