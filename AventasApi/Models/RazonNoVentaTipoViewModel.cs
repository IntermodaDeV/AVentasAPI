using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class RazonNoVentaTipoViewModel
    {
        public int IdRazonNoVentaTipo { get; set; }
        public string Tipo { get; set; }
        public List<RazonNoVentaCausaViewModel> RazonesNoVenta ;

        public RazonNoVentaTipoViewModel()
        {
            RazonesNoVenta = new List<RazonNoVentaCausaViewModel>();
        }
    }
}