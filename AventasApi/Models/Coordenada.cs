using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class Coordenada
    {
        public string ACCOUNT { get; set; }
        public string NAME { get; set; }
        public string COMPANY { get; set; }
        public decimal LATITUDE { get; set; }
        public decimal LONGITUD { get; set; }
    }
}