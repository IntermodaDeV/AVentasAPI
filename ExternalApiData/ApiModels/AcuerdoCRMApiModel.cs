using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class AcuerdoCRMApiModel
    {
        public string CUSTOMER_ACCOUNT { get; set; }
        public string ID_SALES_AGREEMENT { get; set; }
        public string CLASS_SALES_AGREEMENT { get; set; }
        public string PAYMENT { get; set; }
        public string LINE { get; set; }
        public string LINENAME { get; set; }
        public string STARTDATE { get; set; }
        public string ENDDATE { get; set; }
        public string CURRENCY { get; set; }
        public string ITEM_NAME { get; set; }
        public string AMOUNT { get; set; }
        public string REMAINING { get; set; }
        public string RELEASED { get; set; }
        public string INVOICED { get; set; }
        public string DELIVERED { get; set; }
        public string ENTITY { get; set; }
        public string CUST_DISCLINE_GROUP { get; set; }
    }
}