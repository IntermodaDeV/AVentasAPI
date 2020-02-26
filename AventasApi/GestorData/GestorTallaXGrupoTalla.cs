using AventasApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorTallaXGrupoTalla
    {
        
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/GrupoTalla";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorTallaXGrupoTalla()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<GrupoTalla> gruposTalla = new List<GrupoTalla>();

                using (AVentasEntities context = new AVentasEntities())
                {
                    gruposTalla = context.GrupoTalla.ToList();

                }

                if (gruposTalla != null && gruposTalla.Count > 0)
                {
                    var taskGetTallasXGrupoTalla =
                        gruposTalla.Select(async col =>
                        {
                            var Credentials = new Dictionary<string, string> {
                                { "userName", "desarrollo" },
                                { "password", "Intermoda2020" },
                                { "GrupoTalla", col.CodigoGrupoTalla },
                            };
                            List<TallaPorGrupoTalla> tallasXGrupoTalla = new List<TallaPorGrupoTalla>();
                            var content = new FormUrlEncodedContent(Credentials);
                            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                tallasXGrupoTalla = await response.Content.ReadAsAsync<List<TallaPorGrupoTalla>>();
                                tallasXGrupoTalla.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        context.TallasXGrupo.Add(new TallasXGrupo
                                        {
                                            CodigoTalla = txg.codigo,
                                            CodigoGrupoTalla = col.CodigoGrupoTalla,
                                            Orden = (int)float.Parse(txg.description)
                                        });
                                        context.SaveChanges();
                                    }
                                });


                            }
                            else
                            {
                                Debug.WriteLine("Error");

                            }

                        });
                    await Task.WhenAll(taskGetTallasXGrupoTalla);
                    Debug.WriteLine("Finalizo");

                }


            });
        }
    }
}