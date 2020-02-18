using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class EdadesViewModel
    {
        public string IdEdad { get; set; }
        public string Edad { get; set; }
        public Nullable<int> Orden { get; set; }
        public List<ProductoXColeccionViewModel> ProductosXEdad;

    }
}