using AventasApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorLimiteCuentaCorriente
    { 
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}customers/list";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorLimiteCuentaCorriente()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {

           
            TaskActualizarLineas = new Task(async () =>
            {

                List<Clientes> gruposTalla = new List<Clientes>();


                using (AVentasEntities context = new AVentasEntities())
                {
                    gruposTalla = context.Clientes.ToList();
                }

                if (gruposTalla != null && gruposTalla.Count > 0)
                {
                    for (int i = 0; ((i + 1) * 100) < gruposTalla.Count(); i++)
                    {
                        List<Clientes> buffer = new List<Clientes>();
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
                            var Credentials = new Dictionary<string, string> {
                                { "userName", "desarrollo" },
                                { "password", "Intermoda2020" },
                                { "customer", col.CodigoCliente },
                            };
                            List<ClientesApiModel> tallasXGrupoTalla = new List<ClientesApiModel>();
                            var content = new FormUrlEncodedContent(Credentials);
                            //var content = new StringContent(Credentials.ToJson(), Encoding.UTF8, "application/json");
                            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                tallasXGrupoTalla = await response.Content.ReadAsAsync<List<ClientesApiModel>>();
                                tallasXGrupoTalla.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        try
                                        {
                                            foreach (var cuentaCorriente in txg.listacuentacorriente)
                                            {
                                                
                                            context.LimiteCreditoxCliente.Add(new LimiteCreditoxCliente
                                            {
                                               CodigoCliente = col.CodigoCliente,
                                               Valor = cuentaCorriente.valor,
                                               Descripcion = cuentaCorriente.concepto

                                            });
                                            context.SaveChanges();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            throw;
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
                        Debug.WriteLine("Finalizó");

                    }


                    

                }


            });
        }
    }
}