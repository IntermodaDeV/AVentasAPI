using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorColoresXProducto
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";

        public async Task<List<ColorXProductoCRMApiModel>> ObtenerColoresDesdeCRMAPI(string CodigoColeccion)
        {
            var colores = new List<ColorXProductoCRMApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    colores = JsonConvert.DeserializeObject<List<ColorXProductoCRMApiModel>>(response.Content);
                }
            });
            return colores;
        }
    }
}