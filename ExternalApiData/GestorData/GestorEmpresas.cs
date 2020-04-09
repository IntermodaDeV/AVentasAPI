using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorEmpresas
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}empresa/empresas";

        public async Task<List<EmpresasCRMApiModel>> ObtenerEmpresasDesdeCRMAPI()
        {
            var empresas = new List<EmpresasCRMApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    empresas = JsonConvert.DeserializeObject<List<EmpresasCRMApiModel>>(response.Content);
                }
            });
            return empresas;
        }
    }
}