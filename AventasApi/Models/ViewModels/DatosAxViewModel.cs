using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DatosAxViewModel
    {
        public string COMPANY { get; set; }
        public string CURRENCYCODE { get; set; }
        public string TRANSDATE { get; set; }
        public string NUMBERINVOCEID { get; set; }
        public string DESCRIPTION { get; set; }
        public double CREDIT { get; set; }
        public string VENDACCOUNT { get; set; }
        public string USERID { get; set; }
        public string JOURNALNAME { get; set; }
        public string OFFSETACCOUNT { get; set; }
        public string SERIE { get; set; }
    }
}