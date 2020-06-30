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
    public class GestorSubFacturasXCliente
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/{{0}}/gmonrroy/{{1}}/0/{{2}}";
        public async Task<List<SubFacturasXClienteApiModel>> ObtenerSubFacturas(string empresa, string asesor, string ClienteId)
        {
            List<SubFacturasXClienteApiModel> subFacturas = new List<SubFacturasXClienteApiModel>();

            string peticion = string.Format(UrlString, empresa, asesor, ClienteId);

            var restClient = new RestClient(peticion);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
                subFacturas = JsonConvert.DeserializeObject<List<SubFacturasXClienteApiModel>>(response.Content);
            if (subFacturas == null)
            {
                subFacturas = new List<SubFacturasXClienteApiModel>();
            }
            return subFacturas;


        }
    }

}