using ExternalApiData.ApiModels;
using ExternalApiData.Enviroments;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorEspecificacionPagos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}specpagos/imhn";

        public async Task<List<EspecificacionPagosCRMApiModel>> ObtenerPagosDesdeCRMAPI()
        {
            var pagos = new List<EspecificacionPagosCRMApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    pagos = JsonConvert.DeserializeObject<List<EspecificacionPagosCRMApiModel>>(response.Content);
                }
            });
            return pagos;
        }
    }
}
