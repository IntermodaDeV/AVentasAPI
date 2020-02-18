using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class ClientesCRMApiModel
    {
        public string ACCOUNT { get; set; }
        public string NAME { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string ENTITY { get; set; }
        public string SALES_AREA { get; set; }
        public string SALES_AREA_NAME { get; set; }
        public string AUTONOMOUS_COMMUNITY { get; set; }
        public string PRICE { get; set; }
        public string PRICE_NAME { get; set; }
        public string TAX_GROUP { get; set; }
        public string TOTAL_DISCOUNT { get; set; }
        public string CUSTOMER_GROUP { get; set; }
        public string CUSTOMER_GROUP_NAME { get; set; }
        public string CURRENCY { get; set; }
        public string CREDIT_LIMIT { get; set; }
        public string CREDIT_AVAILABLE { get; set; }
        public string BLOCKED { get; set; }
        public string VENDOR { get; set; }
        public string FLAG_SEQFACT { get; set; }
    }
}