using ExternalApiData.Enviroments;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using ExternalApiData.Models.ApiModels;
using AventasApi.Utils;

namespace ExternalApiData.GestorData
{
    public class GestorCuentasBancarias
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}cuentasbancarias/imhn";
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<CuentasBancariasCRMApiModel>> ObtenerCuentasDesdeCRMAPI()
        {
            var cuentasBancarias = new List<CuentasBancariasCRMApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    cuentasBancarias = JsonConvert.DeserializeObject<List<CuentasBancariasCRMApiModel>>(response.Content);
                }
            });
            return cuentasBancarias;
        }
    }    
}