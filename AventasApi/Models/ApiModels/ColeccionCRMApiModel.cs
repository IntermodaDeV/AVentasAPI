using AventasApi.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class ColeccionCRMApiModel
    {
        public string PACKAGE { get; set; }
        public string NAME { get; set; }
        public string PACKAGE_TYPE { get; set; }
        public string PACKAGE_TYPE_NAME { get; set; }
        public string START_DATE_DESIGN { get; set; }
        public string END_DATE_DESIGN { get; set; }
        public string START_DATE_PRODUCTION { get; set; }
        public string END_DATE_PRODUCTION { get; set; }
        public string START_DATE_SALES_ORDER_ENTRY { get; set; }
        public string END_DATE_SALES_ORDER_ENTRY { get; set; }
        public string START_DATE_DELIVERY_SALES_ORDER { get; set; }
        public string END_DATE_DELIVERY_SALES_ORDER { get; set; }
        public string ENTITY { get; set; }
        public string STATUS { get; set; }
    }
}