using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorAtributosXProductos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/{{0}}/Estructura";

        public async Task<List<AtributosXProductoCRMApiModel>> ObtenerAtributosDesdeCRMAPI(string CodigoProducto)
        {
            var atributos = new List<AtributosXProductoCRMApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoProducto);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    atributos = JsonConvert.DeserializeObject<List<AtributosXProductoCRMApiModel>>(response.Content);
                }
            });
            return atributos;
        }
    }
}