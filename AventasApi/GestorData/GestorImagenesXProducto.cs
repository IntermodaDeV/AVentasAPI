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
//using IMS.Extensions;
using System.Data.Entity;
using AventasApi.Models.ViewModels;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorImagenesXProducto
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}productos/imhn/{{0}}";/// 1 = productId
        public HttpClient client = new ClienteHttp();
        public async Task<List<FotografiasXProductoViewModel>> ObtenerImagenesXProducto()
        {

            List<Colecciones> colecciones = new List<Colecciones>();
            List<FotografiasXProductoViewModel> fotografiasXProducto = new List<FotografiasXProductoViewModel>();

            using (AVentasEntities context = new AVentasEntities())
            {
                colecciones = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().ToList();
            }


            Parallel.ForEach(colecciones, col =>
            {
                string peticion = string.Format(UrlString, col.CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);
                if (response.IsSuccessful)
                {
                    var imagenes = JsonConvert.DeserializeObject<List<ImageneXProductoXColorApiModel>>(response.Content);
                    List<FotografiasXProductoViewModel> fotografiasXProductoAAgregar = new List<FotografiasXProductoViewModel>();

                    foreach (var imagen in imagenes)
                    {
                        string nombreImagen = "";
                        try
                        {
                            nombreImagen = imagen.IMAGE_PATH.Split('\\').Last();
                        }
                        catch (Exception) { }
                        var producto = col.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == imagen.ITEM_CODE);
                        if (producto != null)
                        {
                            var imagenAGuardar = new FotografiasXProductoViewModel
                            {
                                FotografiaProducto = nombreImagen,
                                IdProducto = producto.IdProducto,
                                CodigoColor = imagen.ITEM_COLOR,
                                Principal = imagen.IMAGE_MAIN == "1"
                            };
                            fotografiasXProductoAAgregar.Add(imagenAGuardar);

                        }
                    }
                    lock (fotografiasXProducto)
                    {
                        fotografiasXProducto.AddRange(fotografiasXProductoAAgregar);
                    }
                }
            });

            return fotografiasXProducto;
        }
        public async Task GuardarImagenesXProducto(List<FotografiasXProductoViewModel> fotografiasAguardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {

                var connectionString = "data source=209.126.64.158,49170;initial catalog=AventasTesting20200206;persist security info=True;user id=developer;password=D3vCitHn.20!8;MultipleActiveResultSets=True;App=EntityFramework&quot;";


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