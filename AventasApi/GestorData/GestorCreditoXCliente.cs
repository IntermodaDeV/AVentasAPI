using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.GestorData
{
    public class GestorCreditoXCliente
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}clientes/imhn/{0}/{1}";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorCreditoXCliente()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                //List<Clientes> clientes = new List<Clientes>();
                var clientes = new List<AsesorXClienteViewModel>();

                using (AVentasEntities context = new AVentasEntities())
                {
                    //clientes = context.Clientes.ToList();
                    clientes = context.RutasxAsesor.SelectMany(rutAse => rutAse.Rutas.ClientesxRuta).Select(cliRut => new AsesorXClienteViewModel
                    {
                        CodigoAsesor = cliRut.Rutas.RutasxAsesor.FirstOrDefault().CodigoAsesor,
                        ClienteId = cliRut.CodigoCliente
                    }).ToList();
                }

                if (clientes != null && clientes.Count > 0)
                {
                    for (int i = 0; ((i + 1) * 100) < clientes.Count(); i++)
                    {
                        List<AsesorXClienteViewModel> buffer = new List<AsesorXClienteViewModel>();
                        if (i + 1 * 100 > clientes.Count())
                        {
                            buffer = clientes.GetRange(i * 100, clientes.Count() - ((i - 1) * 100));

                        }
                        else
                        {
                            buffer = clientes.GetRange(i * 100, 100);


                        }
                        var taskGetacuerdos =
                        buffer.Select(async col =>
                        {
                            List<ClientesCRMApiModel> facturasXCliente = new List<ClientesCRMApiModel>();
                            HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoAsesor, col.ClienteId)).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                facturasXCliente = await response.Content.ReadAsAsync<List<ClientesCRMApiModel>>();
                                if (facturasXCliente != null && facturasXCliente.Count > 0)
                                {
                                    facturasXCliente.ForEach(txg =>
                                    {
                                        using (AVentasEntities context = new AVentasEntities())
                                        {
                                            try
                                            {
                                                var cliente =
                                                        context.Clientes.FirstOrDefault(cli => cli.CodigoCliente == txg.ACCOUNT);
                                                cliente.LimiteCredito = Decimal.Parse(txg.CREDIT_LIMIT);
                                                cliente.CreditoDisponible = Decimal.Parse(txg.CREDIT_AVAILABLE);
                                                context.SaveChanges();
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine(e);

                                            }

                                        }

                                    });
                                }

                            }
                            else
                            {
                                Debug.WriteLine("Error en a peticion");

                            }


                        });
                        await Task.WhenAll(taskGetacuerdos);
                    }
                }
            });


            Debug.WriteLine("FFinalizo");
        }
    }

}
