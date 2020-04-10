using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using ExternalApiData.Models;
using ExternalApiData.Models.ApiModels;
using RestSharp;
using Newtonsoft.Json;
using AventasApi.Utils;
using DBData.Database;

namespace ExternalApiData.GestorData
{
    public class GestorAsesores
    {
        private readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/AsesoresDisponibles";

        public async Task<List<AsesorApiModel>> ObtenerAsesoresDesdeCRMAPI()
        {
            var asesores = new List<AsesorApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    asesores = JsonConvert.DeserializeObject<List<AsesorApiModel>>(response.Content);
                }
            });
            return asesores;
        }
    }
}
