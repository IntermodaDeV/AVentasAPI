using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Globalization;

namespace ExternalApiData.GestorData
{
    public class GestorColecciones
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}api/paquetes/imhn/";
        public async Task<List<ColeccionCRMApiModel>> ObtenerColecciones()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var coleccionesDeserializadas = JsonConvert.DeserializeObject<List<ColeccionCRMApiModel>>(response.Content);
            return coleccionesDeserializadas;
        }
       
    }
}