using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorGrupoTalla
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/GruposTallas";
        public async Task<List<GrupoTallaApiModel>> ObtenerGruposTalla()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var lineasDeserializadas = JsonConvert.DeserializeObject<List<GrupoTallaApiModel>>(response.Content);
            return lineasDeserializadas;
        }
    }
}