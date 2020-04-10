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
    public class GestorProductos
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/Details";
        public async Task<List<ProductoXColeccionApiModel>> Obtener(string codigoColeccion)
        {
            string peticion = string.Format(UrlString);
            var restClient = new RestClient(peticion)
            {
                Timeout = 600 * 1000
            };
            var request = new RestRequest(Method.POST);
            ProductosCredentials credentials = new ProductosCredentials
            {
                userName = "desarrollo",
                password = "Intermoda2020",
                seasonid = codigoColeccion
            };
            request.AddJsonBody(credentials);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            var listaDeserializada = JsonConvert.DeserializeObject<List<ProductoXColeccionApiModel>>(response.Content);
            return listaDeserializada;
        }
    }
    public class ProductosCredentials
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string seasonid { get; set; }
    }
}