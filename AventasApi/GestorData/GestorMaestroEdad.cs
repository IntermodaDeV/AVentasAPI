using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models;
using AventasApi.Models.ApiModels;

namespace AventasApi.GestorData
{
    public static class GestorMaestroEdad
    {
        private static string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/Details";
        private static HttpClient client = new ClienteHttp();
        public static bool ProductosActualizados = false;
        public static bool ErrorAlActualizar = false;
        //AVentasEntities context = new AVentasEntities();

        //public GestorProductos()
        //{
        //}
        public static async Task ActualizarProductos()
        {
            try
            {
                Random randomColorNumber = new Random();
                List<Colecciones> colecciones = new List<Colecciones>();
                using (AVentasEntities context = new AVentasEntities())
                {
                    colecciones = context.Colecciones.ToList();

                }

                List<MaestroEdad> maestroedad = new List<MaestroEdad>();
                //List<ProductoXColeccionApiModel> productos = new List<ProductoXColeccionApiModel>();
                List<Task> taskColecciones = colecciones.Select(coleccion =>
                    Task.Run(async () =>
                        {
                            var credentials = new Dictionary<string, string>
                     {
                     { "userName", "desarrollo" },
                     {  "password", "Intermoda2020" },
                     {"seasonid", coleccion.CodigoColeccion}
                    };
                            var content = new FormUrlEncodedContent(credentials);
                            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                var productosColeccion = await response.Content.ReadAsAsync<List<ProductoXColeccionApiModel>>();

                                productosColeccion.ForEach(async prod =>
                                {
                                    int codProd = 0;
                                    if (prod.linea != "" && prod.grupoTallaId != "")
                                    {
                                        lock (maestroedad)
                                        {
                                            if (prod.edad != null && prod.edad.Count > 0)
                                            {
                                                MaestroEdad edad = new MaestroEdad
                                                {
                                                    IdEdad = prod.edad[0].codigo,
                                                    Edad = prod.edad[0].description,
                                                };
                                                var e = maestroedad.FirstOrDefault(me=> me.IdEdad==edad.IdEdad && me.Edad ==edad.Edad);
                                                if (e == null)
                                                {
                                                    maestroedad.Add(edad);

                                                }
                                               
                                               
                                            }
                                        }



                                    }

                                    //productos.Add(prod);



                                });

                            }

                        })
                ).ToList();

                Task cargarProductos = Task.WhenAll(taskColecciones);
                cargarProductos.Wait();
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.MaestroEdad.AddRange(maestroedad.Distinct());
                    context.SaveChanges();

                }
                ProductosActualizados = true;

                //GestorImagenesXProducto gi = new GestorImagenesXProducto();
                //gi.ActualizarImagenes();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


    }
}