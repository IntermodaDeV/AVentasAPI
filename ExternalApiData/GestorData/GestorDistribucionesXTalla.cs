using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ExternalApiData.GestorData
{
    public class GestorDistribucionesXTalla
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/distribucion";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorDistribucionesXTalla()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {

            //List<TallasXGrupo> tallasXGrupos = new List<TallasXGrupo>();
            //using (AVentasEntities context = new AVentasEntities())
            //{
            //    tallasXGrupos = context.TallasXGrupo.ToList();
            //}
            TaskActualizarLineas = new Task(async () =>
            {

                //List<Clientes> clientes = new List<Clientes>();
                var distribuciones = new List<DistribucionXTallaApiModel>();
                HttpResponseMessage response = await client.GetAsync(UrlString).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    distribuciones = await response.Content.ReadAsAsync<List<DistribucionXTallaApiModel>>();
                }
                if (distribuciones != null && distribuciones.Count > 0)
                {
                    var taskGetacuerdos =
                    distribuciones.Select(async col =>
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            try
                            {
                                if (col.DIST_QTY != "0.00" && col.DIST_QTY != ".00")
                                {
                                    //Debug.WriteLine(col.DIST_QTY);
                                    var tallaEmpaque = context.TallasXGrupo.FirstOrDefault(tallXGrup => col.SIZE_CHART == tallXGrup.GrupoTalla.CodigoGrupoTalla && tallXGrup.CodigoTalla == col.DISTRIBUTION);
                                    if (tallaEmpaque != null)
                                    {
                                    //var distribucionDB = context.DistribucionxTalla.FirstOrDefault(distr => tallaEmpaque.IdTallaxGrupo == distr.IdTallaxGrupo && distr.NombreTalla == col.DIST_SIZE);
                                    if (false)
                                    {
                                        //distribucionDB.NombreDistribucion = distribucionDB.NombreDistribucion;
                                        //distribucionDB.Cantidad = distribucionDB.Cantidad;
                                    }
                                    else
                                    {

                                        DistribucionxTalla distribucion = new DistribucionxTalla
                                        {
                                            IdTallaxGrupo = tallaEmpaque.IdTallaxGrupo,
                                            NombreDistribucion = col.DIST_NAME,
                                            NombreTalla = col.DIST_SIZE,
                                            Cantidad = col.DIST_QTY,
                                        };
                                        context.DistribucionxTalla.Add(distribucion);
                                    }
                                    context.SaveChanges();
                                }
                                }
                                else
                                {
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    });
                    await Task.WhenAll(taskGetacuerdos);

                    Debug.WriteLine("FFinalizo");
                }
            });
        }
    }
}
