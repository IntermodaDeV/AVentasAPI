using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class TallasXProductoApiModel
    {
        public string codigo { get; set; }
        public string grupoTallaId { get; set; }
        public decimal orden{ get; set; }
    }
}