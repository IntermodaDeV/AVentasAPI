using ExternalApiData.Enviroments;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorTipoPagos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}tipopagos/imhn";

        public async Task<List<TiposPagoCRMApiModel>> ObtenerTiposDesdeCRMAPI()
        {
            var listaTipos = new List<TiposPagoCRMApiModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    listaTipos = JsonConvert.DeserializeObject<List<TiposPagoCRMApiModel>>(response.Content);
                }
            });
            return listaTipos;
        }
    }
}