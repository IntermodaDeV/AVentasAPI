using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;

namespace ExternalApiData.GestorData
{
    public class GestorTipoPaquete
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn";

        public async Task<List<ColeccionesCRMApiViewModel>> ObtenerTiposDesdeCRMAPI()
        {
            var colecciones = new List<ColeccionesCRMApiViewModel>();
            await Task.Run(() =>
            {
                var restClient = new RestClient(UrlString);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    colecciones = JsonConvert.DeserializeObject<List<ColeccionesCRMApiViewModel>>(response.Content);
                }
            });
            return colecciones;
        }
    }
}