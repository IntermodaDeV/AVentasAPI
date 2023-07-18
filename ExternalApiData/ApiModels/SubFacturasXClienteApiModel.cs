using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class SubFacturasXClienteApiModel
    {
        public string ENTITY { get; set; }
        public string ACCOUNT_NUM { get; set; }
        public string INVOICE { get; set; }
        public string AMOUNT_MST { get; set; }
        public string AMOUNT_CUR { get; set; }
        public string DUE_DATE { get; set; }
        public string LIMIT_DATE { get; set; }
        public string DISC_DATE { get; set; }
        public string DISC_AMOUNT { get; set; }
        public string PAYM_AMOUNT { get; set; }
        public string REF_TRANSOPEN { get; set; }
        public string REF_CUSTTRANS { get; set; }
        public string AGREEMENT_NUM { get; set; }
        public string PA_PAYM_NUM { get; set; }
        public string PA_PAYM_AMOUNT { get; set; }
        public string PA_DUE_AMOUNT { get; set; }
        public string PA_REF_APSA { get; set; }
        public string CURRENCY_CODE { get; set; }
        public string AGREEMENT_NAME { get; set; }
        public string FACTURACION_FEL { get; set; }
        public string FREIGHT { get; set; }
    }
}