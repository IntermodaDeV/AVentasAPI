using ExternalApiData.Enviroments;
using ExternalApiData.GestorData;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ExternalApiData.GestorData
{
    public class GestorRutasXAsesor
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/{{0}}/{{1}}/rutas";

        public async Task<List<RutasxAsesor>> ObtenerRutasXAsesor()
        {
            List<Asesores> asesores = new List<Asesores>();
            List<RutasxAsesor> rutasXAsesorXAgregar = new List<RutasxAsesor>();
            using (AVentasEntities context = new AVentasEntities())
            {
                asesores = context.Asesores.AsNoTracking().ToList();
            }
            Parallel.ForEach(asesores, ase =>
            {

                List<RutasXAsesorApiModel> rutas = new List<RutasXAsesorApiModel>();
                var arregloString = ase.CodigoAsesor.Split('-');
                string peticion = string.Format(UrlString, ase.EmpresaId, ase.Diario);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    rutas = JsonConvert.DeserializeObject<List<RutasXAsesorApiModel>>(response.Content);
                    foreach (var rutaXAsesor in rutas)
                    {
                        //if (rutaXAsesor.CODE.Length > 1)
                        //{
                            var rutaXAsesorXAgregar = new RutasxAsesor
                            {
                                CodigoRuta = rutaXAsesor.ENTITY.ToLower() + "-" + rutaXAsesor.CODE,
                                CodigoAsesor = ase.CodigoAsesor,

                            };
                            lock (rutasXAsesorXAgregar)
                            {
                                rutasXAsesorXAgregar.Add(rutaXAsesorXAgregar);
                            }
                        //}
                    }
                }
            });
            return rutasXAsesorXAgregar;


        }
        public async Task GuardarRutasXAsesor(List<RutasxAsesor> rutasXAsesorXAgregar)
        {
            rutasXAsesorXAgregar.ForEach(asa => Debug.WriteLine(asa.CodigoRuta));
            using (AVentasEntities context = new AVentasEntities())
            {
                context.RutasxAsesor.AddRange(rutasXAsesorXAgregar);
                await context.SaveChangesAsync();
            }
        }
    }
}