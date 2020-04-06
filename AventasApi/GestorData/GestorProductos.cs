using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using DBData.Database;
using AventasApi.Models;
using AventasApi.Models.ApiModels;

namespace AventasApi.GestorData
{
    public static class GestorProductos
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


                                        ProductosxColeccion producto = new ProductosxColeccion
                                        {
                                            CodigoProducto = prod.productoId,
                                            IdColeccion = coleccion.IdColeccion,
                                            NombreProducto = prod.nombreProducto,
                                            BackOrder = prod.backorder == 0 ? false : true,
                                            Multiplo = prod.multiplo,
                                            IdLinea = prod.linea,
                                            CodigoGrupoTalla = prod.grupoTallaId,

                                        };
                                        if (prod.edad != null && prod.edad.Count > 0)
                                        {

                                            producto.IdEdad = prod.edad[0].codigo;
                                        }
                                        try
                                        {
                                            using (AVentasEntities con = new AVentasEntities())
                                            {
                                                var linea = con.MaestroLinea.FirstOrDefault(ml => ml.Linea == prod.linea);
                                                producto.IdLinea = linea.IdLinea;
                                                con.ProductosxColeccion.Add(producto);
                                                con.SaveChanges();
                                                codProd = producto.IdProducto;

                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e);

                                            //productosDuplicados.Add(prod);

                                        }
                                    }

                                    if (codProd != 0)
                                    {




                                        Task taskImagenes = Task.WhenAll(prod.listaImagenes.Select(img =>
                                             Task.Run(async () =>
                                             {
                                                 using (AVentasEntities con = new AVentasEntities())
                                                 {
                                                     if (MimeMapping.GetMimeMapping(img.description).Contains("image"))
                                                     {
                                                         try
                                                         {
                                                             con.FotografiasXProducto.Add(new FotografiasXProducto()
                                                             {
                                                                 Codigo = img.codigo,
                                                                 Descripcion = img.description,
                                                                 IdProducto = codProd
                                                             });
                                                             con.SaveChanges();
                                                         }
                                                         catch (Exception e)
                                                         {
                                                             Console.WriteLine(e);
                                                         }
                                                     }
                                                 }
                                             })
                                        ));

                                        await Task.WhenAll(taskImagenes);
                                        Debug.WriteLine("Finalizo Carga de Fotografias");
                                    }
                                    else
                                    {

                                    }
                                    //productos.Add(prod);



                                });

                            }

                        })
                ).ToList();

                Task cargarProductos = Task.WhenAll(taskColecciones);
                cargarProductos.Wait();
                ProductosActualizados = true;
                Debug.WriteLine("Finalizo");
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