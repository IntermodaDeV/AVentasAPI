using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalApiData.ApiModels
{
    public class DireccionesCRMApiModel
    {
        public long POSTALADDRESS { get; set; }
        public string LOCATIONNAME { get; set; }
        public string ADDRESS { get; set; }
        public int ISPRIMARY { get; set; }
    }
}
