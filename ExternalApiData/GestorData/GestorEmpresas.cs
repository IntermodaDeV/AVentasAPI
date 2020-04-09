using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using AventasApi.Utils;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorEmpresas
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}empresa/empresas";
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<EmpresasCRMApiModel>> ObtenerEmpresas()
        {
            List<EmpresasCRMApiModel> empresasCRMs = new List<EmpresasCRMApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    empresasCRMs = JsonConvert.DeserializeObject<List<EmpresasCRMApiModel>>(response.Content);
                }
            });
            return empresasCRMs;
        }
    }
}