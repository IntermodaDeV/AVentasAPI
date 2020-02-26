using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using AventasApi.Infrastructure;
using MongoDB.Bson;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorAtributos
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/estructura";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorAtributos()
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

                    for (int i = 0; ((i + 1) * 100) < gruposTalla.Count(); i++)
                    {
                        List<ProductosxColeccion> buffer = new List<ProductosxColeccion>();
                        if (i + 1 * 100 > gruposTalla.Count())
                        {
                            buffer = gruposTalla.GetRange(i * 100, gruposTalla.Count() - ((i - 1) * 100));

                        }
                        else
                        {
                            buffer = gruposTalla.GetRange(i * 100, 100);


                        }

                        var taskGetTallasXGrupoTalla =
                            buffer.Select(async col =>
                            {
                                var Credentials = new Dictionary<string, string>
                                {
                                    {"userName", "desarrollo"},
                                    {"password", "Intermoda2020"},
                                    {"ItemID", col.CodigoProducto},
                                };
                                List<AtributosApiModel> tallasXGrupoTalla = new List<AtributosApiModel>();
                                var content = new FormUrlEncodedContent(Credentials);
                                HttpResponseMessage response =
                                    await client.PostAsync(UrlString, content).ConfigureAwait(false);
                                if (response.IsSuccessStatusCode)
                                {
                                    tallasXGrupoTalla = await response.Content.ReadAsAsync<List<AtributosApiModel>>();
                                    tallasXGrupoTalla.ForEach(txg =>
                                    {
                                        using (AVentasEntities context = new AVentasEntities())
                                        {
                                            context.AtributosxProducto.Add(new AtributosxProducto
                                            {
                                                CodigoAtributo = txg.codigo,
                                                IdProducto = col.IdProducto,
                                                Descripcion1 = txg.description,
                                                Descripcion2 = txg.description2

                                            });
                                            context.SaveChanges();
                                        }
                                    });


                                }
                                else
                                {
                                    Debug.WriteLine("Error en a peticion");
                                    Debug.WriteLine(response.ToJson());

                                }

                            });
                        await Task.WhenAll(taskGetTallasXGrupoTalla);
                    }
                    Debug.WriteLine("Finalizo");

                    //for (int i = 0;(i* 100) <taskGetTallasXGrupoTalla.Count(); i ++)
                    //{
                    //    if (i+1 * 100>taskGetTallasXGrupoTalla.Count())
                    //    {

                    //    var buffer = taskGetTallasXGrupoTalla.GetRange(i, taskGetTallasXGrupoTalla.Count()-((i-1) * 100));
                    //    await Task.WhenAll(buffer);
                    //    }
                    //    else
                    //    {
                    //    var buffer = taskGetTallasXGrupoTalla.GetRange(i, i+1 * 100);

                    //    }
                    //}
                }

              

            });
        }
    }
}