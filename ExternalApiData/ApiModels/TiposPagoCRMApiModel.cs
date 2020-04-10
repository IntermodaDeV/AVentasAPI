using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class TiposPagoCRMApiModel
    {
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string TYPE { get; set; }
        public string COMPANY_CODE { get; set; }
    }
}