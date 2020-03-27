using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TallaPorProductoViewModel
    {
        public int id { get; set; }

        public string PRODUCT { get; set; }

        public string SIZEGROUP { get; set; }

        public string SIZE { get; set; }
    }
}