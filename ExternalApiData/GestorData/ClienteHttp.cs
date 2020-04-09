using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace ExternalApiData.GestorData
{
    public class ClienteHttp : HttpClient
    {
        public ClienteHttp()
        {
            this.Timeout = TimeSpan.FromSeconds(601.00);
        }
    }
}