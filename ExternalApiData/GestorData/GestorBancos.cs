using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using AventasApi.Utils;

namespace ExternalApiData.GestorData
{
    public class GestorBancos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}bancos/imhn";
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public static async Task<List<BancoApiModel>> ObtenerBancos()
        {
            List<BancoApiModel> listaBancos = new List<BancoApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    listaBancos = JsonConvert.DeserializeObject<List<BancoApiModel>>(response.Content);
                }
            });
            return listaBancos;
        }
    }
}