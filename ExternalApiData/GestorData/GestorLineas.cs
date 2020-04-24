using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorLineas
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/LineasProductos";
        public async Task<List<LineaApiModel>> ObtenerLineas()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var lineasDeserializadas = JsonConvert.DeserializeObject<List<LineaApiModel>>(response.Content);
            return lineasDeserializadas;
        }
    }
}