using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using ExternalApiData.Enviroments;
using AventasApi.Utils;

namespace ExternalApiData.GestorData
{
    public class GestorImagenesXProducto
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/{{0}}";
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<ImageneXProductoXColorApiModel>> ObtenerImagenesDesdeCRMAPI(string CodigoColeccion)
        {
            var imagenes = new List<ImageneXProductoXColorApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);
                if (response.IsSuccessful)
                {
                    imagenes = JsonConvert.DeserializeObject<List<ImageneXProductoXColorApiModel>>(response.Content);
                }
            });
            return imagenes;
        }
    }
}