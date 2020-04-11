using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using ExternalApiData.Models;
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