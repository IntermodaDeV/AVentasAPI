using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class PedidoCRMApiModel
    {
        public PedidoCRMApiModel()
        {
            PedidoJsonItems = new List<PedidoJsonItems>();
        }
        public string COMPANY { get; set; }
        public string USER { get; set; }
        public string REFERENCE { get; set; }
        public string CUSTOMER_ACCOUNT { get; set; }
        public string SALES_ORDER_TYPE { get; set; }
        public string PACKAGE_TYPE { get; set; }
        public string PACKAGE { get; set; }
        public string LINE { get; set; }
        public string ID_SALES_AGREEMENT { get; set; }
        public string DATE_CONFIRMED_RECEIPT { get; set; }
        public string DELIVERY_MODE { get; set; }
        public string SALES_MANAGER { get; set; }
        public string DELIVERY_ADDRESS { get; set; }
        public string OBSERVATIONS { get; set; }
        public string DISC_GROUP { get; set; }
        public string SALES_NAME { get; set; }
        public string FISCAL_DOCUMENT { get; set; }
        public string PHONE { get; set; }
        public string INCLUDE_TAX { get; set; }

        public List<PedidoJsonItems> PedidoJsonItems { get; set; }
    }
    public class PedidoJsonItems
    {
        public string REFERENCE { get; set; }
        public string ITEM_CODE { get; set; }
        public string COLOR { get; set; }
        public string QUANTITY { get; set; }
        public string UNIT { get; set; }
        public string UNIT_PRICE { get; set; }
        public string SIZE { get; set; }
        public string LOT_NUMBER { get; set; }
        public string DELIVERY_ADDRESS { get; set; }
        public string DISC_PERCENTAGE { get; set; }
    }
}