using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Infrastructure;
using AventasApi.Models;

namespace AventasApi.GestorData
{
    public static class GestorColecciones
    {
        private static string UrlString = @"http://190.109.223.244:8084/api/collection/List";
        public static HttpClient client = new ClienteHttp();
        public static bool ColeccionesActualizadas = false;
        public static bool ErrorAlActualizar = false;

        public static Task TaskActualizarColecciones;
        //public static Task ActualizarColecciones= taskActualizarColecciones();
        //AVentasEntities context = new AVentasEntities();
        static Dictionary<string, string> Credentials = new Dictionary<string, string> {
            { "userName", "desarrollo" },
            { "password", "Intermoda2020" }

        };

        static GestorColecciones()
        {
            ReiniciarTaskActualizarColecciones();

        }
        public static async void ReiniciarTaskActualizarColecciones()
        {


            TaskActualizarColecciones = new Task(async () =>

              {
                  var content = new FormUrlEncodedContent(Credentials);
                  try
                  {
                    //List<Colecciones> coleccionesBd = null;

                    List<ColeccionApiModel> colecciones = null;
                   

                    HttpResponseMessage response = null;
                      response = await client.PostAsync(UrlString, content).ConfigureAwait(false);


                      if (response != null && response.IsSuccessStatusCode)
                      {
                          colecciones = await response.Content.ReadAsAsync<List<ColeccionApiModel>>();
                          var taskGetColecciones =
                              colecciones.Select(async col =>

                              {
                                  Colecciones coleccion = new Colecciones
                                  {
                                      CodigoColeccion = col.codigoColeccion,
                                      Nombre = col.nombre,
                                      ColeccionTipo = col.coleccionTipo,
                                      EmpresaId = col.empresaId,
                                      DisenoInicio = col.disenoInicio,
                                      DisenoFinal = col.disenoFinal,
                                      EntregaInicio = col.entregaInicio,
                                      EntregaFinal = col.entregaFinal,
                                      Estatus = col.estatus,
                                      ProduccionInicio = col.produccionInicio,
                                      ProduccionFinal = col.produccionFinal,
                                      VentaInicio = col.ventaInicio,
                                      VentaFinal = col.ventaFinal,
                                  };
                                  using (AVentasEntities context = new AVentasEntities())
                                  {
                                      try
                                      {
                                          context.Colecciones.Add(coleccion);
                                          context.SaveChanges();
                                          foreach (var lineasxColeccion in col.listaLineas)
                                          {
                                              context.LineasxColeccion.Add(new LineasxColeccion
                                              {
                                                  IdLinea = lineasxColeccion.codigo,
                                                  IdColeccion = coleccion.IdColeccion
                                              });
                                              context.SaveChanges();

                                          }
                                      }
                                      catch (Exception e)
                                      {

                                      }
                                  }
                              });

                          await Task.WhenAll(taskGetColecciones);
                          ColeccionesActualizadas = true;

                      }
                      else
                      {
                          ErrorAlActualizar = true;
                      }
                  }
                  catch (Exception e)
                  {
                      Debug.WriteLine(e);
                    //throw;
                }
              });
        }
    }
}