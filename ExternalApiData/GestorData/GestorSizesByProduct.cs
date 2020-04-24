using AventasApi.Utils;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorSizesByProduct
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";

        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task<List<TallaXProductoCRMApiModel>> ObtenerTallasDesdeCRMAPI(string CodigoCollecion)
        {
            var tallas = new List<TallaXProductoCRMApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoCollecion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    tallas = JsonConvert.DeserializeObject<List<TallaXProductoCRMApiModel>>(response.Content);
                
                }
            });

            if (logicValidation.ValidateDataCount(tallas.Count))
            {
                var listaTallas = tallas.GroupBy(x => new { x.PRODUCT }).Select(g => g.First()).ToList();
                return listaTallas;
            }
            return tallas;
        }
    }
}