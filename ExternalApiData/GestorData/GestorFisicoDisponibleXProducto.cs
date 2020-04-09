using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Models.ApiModels;
using System.Diagnostics;
using DBData.Database;
using ExternalApiData.Enviroments;

namespace ExternalApiData.GestorData
{
    public class GestorFisicoDisponibleXProducto
    {

        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/disponible";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorFisicoDisponibleXProducto()
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
                    for (int i = 0; (i * 100) < gruposTalla.Count(); i++)
                    {
                        List<ProductosxColeccion> buffer = new List<ProductosxColeccion>();
                        if ((i + 1) * 100 > gruposTalla.Count())
                        {
                            buffer = gruposTalla.GetRange(i * 100, gruposTalla.Count() - (i * 100));

                        }
                        else
                        {
                            buffer = gruposTalla.GetRange(i * 100, 100);


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

                                tallasXGrupoTalla.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        try
                                        {
                                            context.FisicoDisponible.Add(new FisicoDisponible
                                            {
                                                CodigoColor = txg.color,
                                                CodigoTalla = txg.talla,
                                                Disponible = txg.fisicaDisponible,
                                                IdProducto = col.IdProducto,
                                                MinStock = 0

                                            });
                                            context.SaveChanges();


                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e);

                                        }

                                    }
                                });


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