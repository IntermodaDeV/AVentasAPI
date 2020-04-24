using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorRutasXAsesor
    {
        private readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/{{0}}/{{1}}/rutas";

        public async Task<List<RutasXAsesorApiModel>> ObtenerRutasDesdeCRMAPI(string EmpresaId, string Diario)
        {
            var rutas = new List<RutasXAsesorApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, EmpresaId, Diario);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    rutas = JsonConvert.DeserializeObject<List<RutasXAsesorApiModel>>(response.Content);
                }
            });
            return rutas;
        }
    }
}