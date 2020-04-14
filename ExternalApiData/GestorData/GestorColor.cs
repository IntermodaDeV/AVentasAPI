using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorColor
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/Color";
        public async Task<List<ColorApiModel>> Obtener()
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.POST);
            Credentials credentials = new Credentials
            {
                userName = "desarrollo",
                password = "Intermoda2020",
            };
            request.AddJsonBody(credentials);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var listaDeserializada = JsonConvert.DeserializeObject<List<ColorApiModel>>(response.Content);
            return listaDeserializada;
        }
    }
    public class Credentials
    {
        public string userName { get; set; }
        public string password { get; set; }
    }
}