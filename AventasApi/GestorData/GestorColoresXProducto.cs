using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using AventasApi.Infrastructure;

namespace AventasApi.GestorData
{
    public class GestorColoresXProducto
    {
        private static string UrlString = @"http://190.109.223.244:8084/api/collection/disponible";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorColoresXProducto()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<ProductosxColeccion> gruposTalla = new List<ProductosxColeccion>();
             

                using (AVentasEntities context = new AVentasEntities())
                {
                    gruposTalla = context.ProductosxColeccion.ToList();
                }

                if (gruposTalla != null && gruposTalla.Count > 0)
                {
                    for (int i = 0;((i+1)* 100) <gruposTalla.Count(); i++)
                    {
                        List<ProductosxColeccion> buffer = new List<ProductosxColeccion>();
                        if (i+1 * 100>gruposTalla.Count())
                        {
                             buffer = gruposTalla.GetRange(i*100, gruposTalla.Count()-((i-1) * 100));

                        }
                        else
                        {
                             buffer = gruposTalla.GetRange(i*100, 100);
                           

                        }
                        var taskGetTallasXGrupoTalla =
                            buffer.Select(async col =>
                        {
                            var Credentials = new Dictionary<string, string> {
                                { "userName", "desarrollo" },
                                { "password", "Intermoda2020" },
                                { "ItemID", col.CodigoProducto },
                            };
                            List<FisicoDisponibleXProductoApiModel> tallasXGrupoTalla = new List<FisicoDisponibleXProductoApiModel>();
                            var content = new FormUrlEncodedContent(Credentials);
                            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                tallasXGrupoTalla = await response.Content.ReadAsAsync<List<FisicoDisponibleXProductoApiModel>>();
                                var codigoColores = tallasXGrupoTalla.Select(ta => ta.color).Distinct();
                                foreach (var codigoColore in codigoColores)
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {

                                        context.ColoresxProducto.Add(new ColoresxProducto
                                        {
                                            CodigoColor = codigoColore,
                                            IdProducto = col.IdProducto,

                                        });
                                        context.SaveChanges();
                                    }
                                }
                                


                            }
                            else
                            {
                                Debug.WriteLine("Error en a peticion");

                            }

                        });
                        await Task.WhenAll(taskGetTallasXGrupoTalla);

                    }
                    
                    Debug.WriteLine("Finalizo");

                    
                  
                }


            });
        }
    }
}