using ExternalApiData.Infrastructure;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;


using System.Data.Entity;


namespace ExternalApiData.GestorData
{
    public class GestorGruposPrecio
    {

        private static string UrlString = @"http://190.109.223.244:8083/api/clientes/imhn/{0}";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorGruposPrecio()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<Asesores> asesores = new List<Asesores>();
                List<MaestroGrupoPrecio> gruposPrecio = new List<MaestroGrupoPrecio>();
                List<Clientes> clientesAGuardar = new List<Clientes>();
                using (AVentasEntities context = new AVentasEntities())
                {
                    asesores = context.Asesores.AsNoTracking().ToList();
                }

                if (asesores != null && asesores.Count > 0)
                {

                    var taskGetacuerdos =
                        asesores.Select(async col =>
                        {
                            List<MaestroGrupoPrecio> gruposPrecioXAsesor = new List<MaestroGrupoPrecio>();
                            List<ClientesCRMApiModel> facturasXCliente = new List<ClientesCRMApiModel>();
                            HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.Usuario)).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                try
                                {
                                    facturasXCliente = await response.Content.ReadAsAsync<List<ClientesCRMApiModel>>();
                                    if (facturasXCliente == null)
                                    {
                                        facturasXCliente = new List<ClientesCRMApiModel>();
                                    }
                                    facturasXCliente.ForEach(txg =>
                                {
                                    if (!gruposPrecioXAsesor.Any(gp => gp.GrupoPrecio == txg.PRICE))
                                    {
                                        gruposPrecioXAsesor.Add(new MaestroGrupoPrecio
                                        {
                                            GrupoPrecio = txg.PRICE,
                                            Descripcion = txg.PRICE_NAME
                                        });
                                    }
                                });

                                    lock (gruposPrecio)
                                    {
                                        foreach (var gruprec in gruposPrecioXAsesor)
                                        {   
                                            if (!gruposPrecio.Any(gp => gp.GrupoPrecio == gruprec.GrupoPrecio))
                                            {
                                                gruposPrecio.Add(new MaestroGrupoPrecio
                                                {
                                                    GrupoPrecio = gruprec.GrupoPrecio,
                                                    Descripcion = gruprec.Descripcion
                                                });
                                            }
                                        }
                                    }

                                }
                                catch (Exception e)
                                {

                                    Debug.WriteLine(e);
                                }

                            }
                            else
                            {
                                Debug.WriteLine("Error en a peticion");

                            }

                        });
                    await Task.WhenAll(taskGetacuerdos);

                    using (AVentasEntities context = new AVentasEntities())
                    {
                        context.MaestroGrupoPrecio.AddRange(gruposPrecio);
                        context.SaveChanges();
                    }
                    Debug.WriteLine("FFinalizo");
                }
            });
        }
    }
}