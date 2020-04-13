using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorDistribucionesXTalla
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/distribucion";
        public async Task<List<DistribucionXTallaApiModel>> Obtener()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var listaDeserializada = JsonConvert.DeserializeObject<List<DistribucionXTallaApiModel>>(response.Content);
            return listaDeserializada;
        }
    }
}