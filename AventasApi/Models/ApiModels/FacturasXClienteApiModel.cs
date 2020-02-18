using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class FacturasXClienteApiModel
    {
        //public string $id { get; set; }
        public string ENTITY { get; set; }
        public string ACCOUNT_NUM { get; set; }
        public string INVOICE { get; set; }
        public string TRANS_TYPE { get; set; }
        public string DOCUMENT_DATE { get; set; }
        public string AMOUNT_CUR { get; set; }
        public string REMAIN_AMOUNT_CUR { get; set; }
        public string AMOUNT_PENDING { get; set; }
        public string DUE_DATE { get; set; }
        public string DISC_DATE { get; set; }
        public string DISCOUNT { get; set; }
        public string CURRENCY_CODE { get; set; }
        public string STATUS { get; set; }
        public string N_PAYMENTS { get; set; }
        public string REF_TRANS { get; set; }
        public string PROD_LINE { get; set; }
        public string DOC_TYPE { get; set; }
    }
}