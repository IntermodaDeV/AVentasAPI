using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    
    public class DescuentoViewModel
    {
        public DescuentoViewModel()
        {
            this.DescuentoDetalle = new List<DescuentoDetalleViewModel>();
        }

        public string Codigo { get; set; }

        public string Descripcion { get; set; }

        public string Empresa { get; set; }

        public List<DescuentoDetalleViewModel> DescuentoDetalle { get; set; }
    }
}