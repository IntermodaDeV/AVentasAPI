using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using BulkInsert;
using Newtonsoft.Json;
using RestSharp;
using AventasApi.Utils;
using AventasApi.Models.ViewModels;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorImagenesXProducto
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/{{0}}";


        public static async Task ObtenerImagenesXProducto()
        {
            LogicValidation LogicValidation = new LogicValidation();
            List<Colecciones> colecciones = new List<Colecciones>();

            using (AVentasEntities context = new AVentasEntities())
            {
                colecciones = context.Colecciones.ToList();
            }

            foreach (var coleccion in colecciones)
            {
                string peticion = string.Format(UrlString, coleccion.CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);
                if (response.IsSuccessful)
                {
                    var ListaImagenes = JsonConvert.DeserializeObject<List<ImageneXProductoXColorApiModel>>(response.Content);
                    if (LogicValidation.ValidateDataCount(ListaImagenes.Count))
                    {
                        Parallel.ForEach(ListaImagenes, imagen =>
                        {
                            if (LogicValidation.IsDataValid(imagen))
                            {
                                using (AVentasEntities context = new AVentasEntities())
                                {
                                    string nombreImagen = UrlImagen(imagen.IMAGE_PATH);
                                    var producto = context.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == imagen.ITEM_CODE
                                                    && prod.IdColeccion == coleccion.IdColeccion);
                                    if (LogicValidation.IsDataValid(producto))
                                    {
                                        var imagenBD = context.FotografiasXProducto.FirstOrDefault(img => img.IdProducto == producto.IdProducto
                                                        && img.FotografiaProducto == nombreImagen);
                                        if (LogicValidation.IsDataValid(imagenBD))
                                        {
                                            imagen.IMAGE_PATH = nombreImagen;
                                            var imagenModel = new ImageneXProductoXColorApiModel()
                                            {
                                                ITEM_CODE = producto.CodigoProducto,
                                                ITEM_COLOR = imagenBD.CodigoColor,
                                                IMAGE_PATH = imagenBD.FotografiaProducto,
                                                IMAGE_MAIN = (imagenBD.Principal == true) ? "1" : "0",
                                            };

                                            bool resul = EvaluarModelos(imagenModel, imagen);
                                            if (!resul)
                                            {
                                                context.Entry(imagenBD).State = EntityState.Modified;
                                                imagenBD.IdProducto = producto.IdProducto;
                                                imagenBD.CodigoColor = imagen.ITEM_COLOR;
                                                imagenBD.FotografiaProducto = nombreImagen;
                                                imagenBD.Principal = (imagen.IMAGE_MAIN == "1") ? true : false;

                                                try
                                                {
                                                    context.SaveChanges();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            FotografiasXProducto nuevaImagen = new FotografiasXProducto()
                                            {
                                                CodigoColor = imagen.ITEM_COLOR,
                                                FotografiaProducto = nombreImagen,
                                                Principal = (imagen.IMAGE_MAIN == "1") ? true : false
                                            };
                                            context.FotografiasXProducto.Add(nuevaImagen);

                                            try
                                            {
                                                context.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex);
                                            }
                                        }
                                    }
                                }
                            }

                        });
                    }
                }
            }
        }

        public static bool EvaluarModelos(object imagenBD, object imagen)
        {
            if (imagenBD == null || imagen == null)
            {
                return false;
            }

            if (imagenBD.GetType() != imagen.GetType())
            {
                return false;
            }

            var Props = imagenBD.GetType().GetProperties();
            foreach (var Prop in Props)
            {
                var aPropValue = Prop.GetValue(imagenBD) ?? string.Empty;
                var bPropValue = Prop.GetValue(imagen) ?? string.Empty;
                if (aPropValue.ToString() != bPropValue.ToString())
                    return false;
            }
            return true;
        }

        public static string UrlImagen(string imagen)
        {
            string nombreImagen = " ";
            try
            {
                nombreImagen = (imagen != null) ? imagen.Split('\\').Last() : " ";
            }
            catch (Exception) { }
            return nombreImagen;
        }

        //public async Task<List<FotografiasXProductoViewModel>> ObtenerImagenesXProducto()
        //{
        //    List<Colecciones> colecciones = new List<Colecciones>();
        //    List<FotografiasXProductoViewModel> fotografiasXProducto = new List<FotografiasXProductoViewModel>();
        //    LogicValidation LogicValidation = new LogicValidation();

        //    using (AVentasEntities context = new AVentasEntities())
        //    {
        //        colecciones = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().ToList();
        //    }

        //    Parallel.ForEach(colecciones, col =>
        //    {
        //        string peticion = string.Format(UrlString, col.CodigoColeccion);
        //        var restClient = new RestClient(peticion);
        //        var request = new RestRequest(Method.GET);
        //        request.AddHeader("Accept", "application/json");
        //        IRestResponse response = restClient.Execute(request);
        //        if (response.IsSuccessful)
        //        {
        //            var ListaImagenes = JsonConvert.DeserializeObject<List<ImageneXProductoXColorApiModel>>(response.Content);
        //            List<FotografiasXProductoViewModel> fotografiasXProductoAAgregar = new List<FotografiasXProductoViewModel>();

        //            if (LogicValidation.ValidateDataCount(ListaImagenes.Count))
        //            {
        //                foreach (var imagen in ListaImagenes)
        //                {
        //                    string nombreImagen = UrlImagen(imagen.IMAGE_PATH);
        //                    var producto = col.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == imagen.ITEM_CODE);
        //                    if (LogicValidation.IsDataValid(producto))
        //                    {
        //                        var imagenAGuardar = new FotografiasXProductoViewModel
        //                        {
        //                            FotografiaProducto = nombreImagen,
        //                            IdProducto = producto.IdProducto,
        //                            CodigoColor = imagen.ITEM_COLOR,
        //                            Principal = imagen.IMAGE_MAIN == "1"
        //                        };
        //                        fotografiasXProductoAAgregar.Add(imagenAGuardar);

        //                        using (AVentasEntities context = new AVentasEntities())
        //                        {
        //                            var imagenBD = context.FotografiasXProducto.FirstOrDefault(img => img.IdProducto == producto.IdProducto
        //                                            && img.FotografiaProducto == nombreImagen);
        //                            if (LogicValidation.IsDataValid(imagenBD))
        //                            {
        //                                var imagenModel = new ImageneXProductoXColorApiModel()
        //                                {
        //                                    ITEM_CODE = producto.CodigoProducto,
        //                                    ITEM_COLOR = imagenBD.CodigoColor,
        //                                    IMAGE_PATH = imagenBD.FotografiaProducto,
        //                                    IMAGE_MAIN = imagenBD.Principal.ToString(),

        //                                };

        //                                if (imagenModel.Equals(imagen))
        //                                {

        //                                }
        //                                else
        //                                {

        //                                }
        //                            }
        //                            else
        //                            {
        //                                FotografiasXProducto nuevaImagen = new FotografiasXProducto()
        //                                {
        //                                    CodigoColor = imagen.ITEM_COLOR,
        //                                    FotografiaProducto = nombreImagen,
        //                                    Principal = Convert.ToBoolean(imagen.IMAGE_MAIN)
        //                                };
        //                                context.FotografiasXProducto.Add(nuevaImagen);
        //                            }

        //                            //try
        //                            //{
        //                            //    context.SaveChanges();
        //                            //}
        //                            //catch (Exception ex)
        //                            //{
        //                            //    Console.WriteLine(ex);
        //                            //}
        //                        }

        //                    }
        //                }
        //                lock (fotografiasXProducto)
        //                {
        //                    fotografiasXProducto.AddRange(fotografiasXProductoAAgregar);
        //                }
        //            }
        //        }
        //    });
        //    return fotografiasXProducto;
        //}

        public async Task GuardarImagenesXProducto(List<FotografiasXProductoViewModel> fotografiasAguardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var connectionString = "data source=209.126.64.158,49170;initial catalog=AventasTesting20200211;persist security info=True;user id=developer;password=D3vCitHn.20!8;MultipleActiveResultSets=True;App=EntityFramework&quot;";

                var transaction = context.Database.BeginTransaction();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.BulkInsert("FotografiasXProducto", fotografiasAguardar);
                }
                transaction.Commit();
            }
        }
    }
}