using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Enviroments;
//using DBData.Database;
using ExternalApiData.Models;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;

namespace ExternalApiData.GestorData
{
    public class GestorClientes
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}clientes/{{0}}/{{1}}";


        public async Task<List<ClientesCRMApiModel>> ObtenerClientesXAsesor(string usuarioAsesor, string empresaId)
        {

           
            string peticion = string.Format(UrlString, usuarioAsesor, empresaId);
            var restClient = new RestClient(peticion);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            if (response.IsSuccessful)
            {
                var clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);
                if (clientes == null)
                    clientes = new List<ClientesCRMApiModel>();
                return clientes;

            }



            return (new List<ClientesCRMApiModel>());
        }
        public async Task<ClientesCRMApiModel> ObtenerClientePorId(string clienteID, string usuario, string empresaId)
        {
            string peticion = string.Format(UrlString, empresaId, usuario) + "/" + clienteID;
            var restClient = new RestClient(peticion);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                var clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);
                if (clientes != null && (clientes.Count() == 1))
                {
                    var clienteCRM = clientes[0];
                    return clienteCRM;
                }
            }
            return null;
        }



    }
}