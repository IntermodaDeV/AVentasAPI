using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TallaPorProductoViewModel
    {
        public int Id { get; set; }

        public string CodProducto { get; set; }

        public string CodTallaGrupo { get; set; }

        public string CodTalla { get; set; }
    }
}