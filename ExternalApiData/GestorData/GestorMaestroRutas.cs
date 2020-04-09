using ExternalApiData.GestorData;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using RestSharp;
using Newtonsoft.Json;
using ExternalApiData.Enviroments;

namespace ExternalApiData.GestorData
{
    public class GestorMaestroRutas
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/{{0}}/{{1}}/rutas";
        


        public async Task<List<Rutas>> ObtenerRutas()
        {
            List<Asesores> asesores = new List<Asesores>();
            List<Rutas> rutasXAgregar = new List<Rutas>();
            using (AVentasEntities context = new AVentasEntities())
            {
                asesores = context.Asesores.AsNoTracking().ToList();
            }
            Parallel.ForEach(asesores, ase =>
              {

                  List<RutasXAsesorApiModel> rutas = new List<RutasXAsesorApiModel>();
                  string peticion = string.Format(UrlString, ase.EmpresaId,ase.Diario);
                  var restClient = new RestClient(peticion);
                  var request = new RestRequest(Method.GET);
                  request.AddHeader("Accept", "application/json");
                  IRestResponse response = restClient.Execute(request);

                  if (response.IsSuccessful)
                  {
                      rutas = JsonConvert.DeserializeObject<List<RutasXAsesorApiModel>>(response.Content);
                      foreach (var rutaXAsesor in rutas)
                      {
                          var rutaAAgregar = new Rutas
                          {
                              CodigoRuta = rutaXAsesor.ENTITY + "-" + rutaXAsesor.CODE,
                              EmpresaId = rutaXAsesor.ENTITY,
                              Nombre = rutaXAsesor.Description
                          };
                          lock (rutasXAgregar)
                          {
                              if (!rutasXAgregar.Any(rutXAgre => rutXAgre.CodigoRuta == rutaAAgregar.CodigoRuta))
                              {
                                  rutasXAgregar.Add(rutaAAgregar);
                              }
                          }
                      }
                  }
              });
            return rutasXAgregar;


        }
        public async Task GuardarRutas(List<Rutas> rutasAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Rutas.AddRange(rutasAGuardar.OrderBy(rut=>rut.EmpresaId).ThenBy(rut => rut.CodigoRuta));
                await context.SaveChangesAsync();
            }
        }
    }
}