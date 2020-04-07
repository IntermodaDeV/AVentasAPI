using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models.ApiModels;

namespace ExternalApiData.GestorData
{
public class GestorTipoPaquete
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}api/paquetes/imhn";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorTipoPaquete()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {


                List<ColeccionesCRMApiViewModel> colecciones = new List<ColeccionesCRMApiViewModel>();
                HttpResponseMessage response = await client.GetAsync(UrlString).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    colecciones = await response.Content.ReadAsAsync<List<ColeccionesCRMApiViewModel>>();
                    List<TiposdeColeccion> tipoColecciones = new List<TiposdeColeccion>();
                    foreach (var coleccion in colecciones)
                    {
                        if (!tipoColecciones.Exists(tipCol => tipCol.ColeccionTipo == coleccion.PACKAGE_TYPE))
                        {
                            tipoColecciones.Add(new TiposdeColeccion
                            {
                                ColeccionTipo = coleccion.PACKAGE_TYPE,
                                Descripcion = coleccion.PACKAGE_TYPE_NAME
                            });
                        }
                        
                    }
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        context.TiposdeColeccion.AddRange(tipoColecciones);
                        context.SaveChanges();
                    }
                    
                    Debug.WriteLine("FFinalizo");
                }
                else
                {
                    Debug.WriteLine("Error en a peticion");

                }
            });
        }
    }
}