using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AventasApi.Utils;
using System.Data.Entity;

namespace AventasApi.GestorData
{
    public class GestorAtributosXProductos
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/{{0}}/Estructura";
        public static bool tallasAgregadas = false;
        public static bool errorAlAgregar = false;

        public static async Task ObtenerAtributosXProducto()
        {
            LogicValidation LogicValidation = new LogicValidation();
            try
            {
                List<ProductosxColeccion> productosLista = new List<ProductosxColeccion>();
                using (AVentasEntities context = new AVentasEntities())
                {
                    productosLista = context.ProductosxColeccion.ToList();
                }

                List<Task> taskColecciones = productosLista.Select(productoXColecion =>
                    Task.Run(async () =>
                    {
                        string peticion = string.Format(UrlString, productoXColecion.CodigoProducto);
                        var restClient = new RestClient(peticion);
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("Accept", "application/json");
                        IRestResponse response = restClient.Execute(request);

                        if (response.IsSuccessful)
                        {
                            List<AtributosXProductoCRMApiModel> productos = JsonConvert.DeserializeObject<List<AtributosXProductoCRMApiModel>>(response.Content);

                            if (LogicValidation.ValidateDataCount(productos.Count))
                            {
                                foreach (var atributo in productos)
                                {
                                    if (LogicValidation.IsDataValid(atributo))
                                    {
                                        using (AVentasEntities context = new AVentasEntities())
                                        {
                                            var atributoBD = context.AtributosxProducto.FirstOrDefault(x=>x.CodigoAtributo == atributo.CODIGO
                                                             && x.IdProducto == productoXColecion.IdProducto);
                                            if (LogicValidation.IsDataValid(atributoBD))
                                            {
                                                context.Entry(atributoBD).State = EntityState.Modified;
                                                atributoBD.CodigoAtributo = atributo.CODIGO;
                                                atributoBD.IdProducto = productoXColecion.IdProducto;
                                                atributoBD.Descripcion1 = atributo.DESCRIPTION;
                                                atributoBD.Descripcion2 = atributo.DESCRIPTION2;
                                            }
                                            else
                                            {
                                                AtributosxProducto atributosxProducto = new AtributosxProducto()
                                                {
                                                    CodigoAtributo = atributo.CODIGO,
                                                    IdProducto = productoXColecion.IdProducto,
                                                    Descripcion1 = atributo.DESCRIPTION,
                                                    Descripcion2 = atributo.DESCRIPTION2,
                                                };
                                                context.AtributosxProducto.Add(atributosxProducto);
                                            }

                                            try
                                            {
                                                await context.SaveChangesAsync();
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                }
                            }
                        }
                    })
                ).ToList();

                Task cargarProductos = Task.WhenAll(taskColecciones);
                cargarProductos.Wait();
                tallasAgregadas = true;
                Debug.WriteLine("Finalizo");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}