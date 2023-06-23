using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Wrappers
{
    public class Response
    {
        public Response()
        {

        }

        public Response(bool success, string message)
        {
            Succeeded = success;
            Message = message;
        }

        public bool Succeeded { get; set; }
        public string Message { get; set; }      
     
    }
}