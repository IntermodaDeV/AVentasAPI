using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;
using ExternalApiData.ApiModels;

namespace ExternalApiData.GestorData
{
    public class GestorTipoPaquete
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}api/paquetes/imhn";

        public async Task<List<TiposDeColeccionDTO>> ObtenerTipoPaqueteDesdeCRMAPI()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var listaDeserializada = JsonConvert.DeserializeObject<List<TiposDeColeccionDTO>>(response.Content);
            return listaDeserializada;
        }
    }
}