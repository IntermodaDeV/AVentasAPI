using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using System.Net.Http.Headers;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorGrupoTalla
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/GrupoTalla";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();
        static Dictionary<string, string> Credentials = new Dictionary<string, string> {
            { "userName", "desarrollo" },
            { "password", "Intermoda2020" },

        };


        static GestorGrupoTalla()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {
            

            TaskActualizarLineas = new Task(async () =>
             {
                 var content = new FormUrlEncodedContent(Credentials);
                 List<LineaApiModel> lineas = new List<LineaApiModel>();

                 HttpResponseMessage response = null;
                 response = await client.PostAsync(UrlString, content).ConfigureAwait(false);

                 if (response != null && response.IsSuccessStatusCode)
                 {
                     lineas = await response.Content.ReadAsAsync<List<LineaApiModel>>();

                     if (lineas != null)
                     {
                         lineas.ForEach(lin =>
                         {

                             using (AVentasEntities context = new AVentasEntities())
                             {
                                 context.GrupoTalla.Add(new GrupoTalla
                                 {
                                     CodigoGrupoTalla = lin.codigo,
                                     Descripcion = lin.description
                                 });
                                 context.SaveChanges();
                             }

                         });
                     }

                 }
                 else
                 {

                 }

             });
        }
    }
}