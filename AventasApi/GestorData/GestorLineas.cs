using AventasApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorLineas
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/Lineas";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();
        static Dictionary<string, string> Credentials = new Dictionary<string, string> {
            { "userName", "desarrollo" },
            { "password", "Intermoda2020" },

        };


        static GestorLineas()
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
                                 context.MaestroLinea.Add(new MaestroLinea
                                 {
                                     IdLinea = lin.codigo,
                                     Linea = lin.description
                                 });
                                 context.SaveChanges();
                             }

                         });
                     }

                 }
                 

             });
        }
    }
}