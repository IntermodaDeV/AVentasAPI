using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ApiModels
{
    public class EmpresasCRMApiModel
    {
        public int id { get; set; }

        public string COMPANY_CODE { get; set; }

        public string NAME { get; set; }

        public string ADDRESS { get; set; }

        public string NIFCIF { get; set; }
    }
}