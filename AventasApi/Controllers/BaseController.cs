using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class BaseController : ApiController
    {

        public string apiUrl = @"http://190.109.223.243:8086/restservices/api/";
        public static HttpClient client = new HttpClient();
    }
}
