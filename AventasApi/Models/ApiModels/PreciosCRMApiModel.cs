using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class PreciosCRMApiModel
    {
        public string PACKAGE { get; set; }
        public string PRODUCT { get; set; }
        public string COLOR { get; set; }
        public string SIZE { get; set; }
        public string PRICEGROUP { get; set; }
        public string PRICE { get; set; }
        public string FROMDATE { get; set; }
        public string TODATE { get; set; }
        public string UNITID { get; set; }
        public string CURRENCY { get; set; }
        public string ENTITY { get; set; }
    }
}