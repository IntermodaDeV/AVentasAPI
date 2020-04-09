using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class FisicoDisponibleXProductoApiModel
    {
        public string color { get; set; }
        public Nullable<decimal> fisicaDisponible { get; set; }
        public string talla { get; set; }
    }
}