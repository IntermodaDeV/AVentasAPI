using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.GestorData
{
    public class GestorEmpresas
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}empresa/empresas";
        public static bool empresasAgregadas = false;
        public static bool errorAlAgregar = false;

        public static async Task ObtenerEmpresas()
        {
            try
            {

                    var restClient = new RestClient(UrlString);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Accept", "application/json");
                    IRestResponse response = restClient.Execute(request);

                    if (response.IsSuccessful)
                    {
                        List<EmpresasCRMApiModel> empresas = JsonConvert.DeserializeObject<List<EmpresasCRMApiModel>>(response.Content);

                        var validarEmpresaEsValida = empresas != null && empresas.Count > 0;
                        if (validarEmpresaEsValida)
                        {

                            foreach (var empresa in empresas)
                            {
                                validarEmpresaEsValida = empresa != null;
                                if (validarEmpresaEsValida)
                                {
                                    
                                }

                            }
                        }
                    }
      

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}