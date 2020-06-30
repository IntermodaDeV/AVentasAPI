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
    public class GestorFacturasXCliente
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/{{0}}/1/{{1}}/{{2}}/FactCliente";
        public async Task<List<FacturasXClienteApiModel>> ObtenerFacturas(string empresa, string usuarioAsesor, string ClienteId)
        {
            List<FacturasXClienteApiModel> facturas = new List<FacturasXClienteApiModel>();

            string peticion = string.Format(UrlString, empresa, usuarioAsesor, ClienteId);

            var restClient = new RestClient(peticion);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
                facturas = JsonConvert.DeserializeObject<List<FacturasXClienteApiModel>>(response.Content);
            if (facturas == null)
            {
                facturas = new List<FacturasXClienteApiModel>();
            }
            return facturas;

        }

    }
}