using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using ExternalApiData.Models;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorColecciones
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/";
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