using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class CuentasBancariasCRMApiModel
    {
        public string CODE { get; set; }
        public string ACCOUNT_NUM { get; set; }
        public string DESCRIPTION { get; set; }
        public string BANK_GROUP { get; set; }
        public string CURRENCY { get; set; }
        public string COMPANY_CODE { get; set; }
    }
}