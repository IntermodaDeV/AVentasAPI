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
//using IMS.Extensions;

namespace ExternalApiData.GestorData
{
    public class GestorPrecioXProducto
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/GrupoPrecios";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorPrecioXProducto()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<ProductosxColeccion> gruposTalla = new List<ProductosxColeccion>();
                List<string> monedas = new List<string>();


                using (AVentasEntities context = new AVentasEntities())
                {
                    gruposTalla = context.ProductosxColeccion.ToList();
                    monedas = context.MaestroMoneda.Select(moneda => moneda.IdMoneda).ToList();
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
                                { "companyId", "IMHN" },
                                { "ItemID", col.CodigoProducto },
                            };
                            List<GestorPrecioXProductoApiModel> tallasXGrupoTalla = new List<GestorPrecioXProductoApiModel>();
                            var content = new FormUrlEncodedContent(Credentials);
                            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                tallasXGrupoTalla = await response.Content.ReadAsAsync<List<GestorPrecioXProductoApiModel>>();

                                tallasXGrupoTalla.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        if (monedas.Contains(txg.moneda))
                                        {

                                            try
                                            {
                                                context.PreciosxProducto.Add(new PreciosxProducto
                                                {
                                                    GrupoPrecio = txg.codigo ,
                                                    IdProducto = col.IdProducto,
                                                    IdMoneda = txg.moneda,
                                                    Precio = txg.precio
                                                });
                                                context.SaveChanges();

                                            }
                                            catch (Exception e)
                                            {

                                                Debug.WriteLine("Error en a peticion");
                                                Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(e));

                                            }
                                        }

                                    }
                                });


                            }
                            else
                            {
                                Debug.WriteLine("Error en a peticion");
                                Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));

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