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
using System.Web;
using System.Data.Entity;
using System.Data.SqlClient;
using BulkInsert;

namespace AventasApi.GestorData
{
    public class GestorSizesByProduct
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";
        public static bool tallasAgregadas = false;
        public static bool errorAlAgregar = false;

        public async Task<List<TallasxProducto>> ObtenerTallasXProducto()
        {
            Random randomColorNumber = new Random();
            List<Colecciones> colecciones = new List<Colecciones>();
            List<TallasxProducto> tallasxProductos = new List<TallasxProducto>();
            using (AVentasEntities context = new AVentasEntities())
            {
                colecciones = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().ToList();
            }

            //List<Task> taskColecciones = colecciones.ForEach(coleccion =>
            foreach (var coleccion in colecciones)
            {
                string peticion = string.Format(UrlString, coleccion.CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    List<TallaXProductoCRMApiModel> tallas = JsonConvert.DeserializeObject<List<TallaXProductoCRMApiModel>>(response.Content);
                    var validarSiTallaEsValida = tallas != null && tallas.Count > 0;
                    if (validarSiTallaEsValida)
                    {
                        //var result = tallas.GroupBy(x => new { x.PRODUCT }).Select(g => g.First());
                        Parallel.ForEach(tallas, talla =>
                         {
                             List<TallasxProducto> tallasxProductosXResult = new List<TallasxProducto>();
                             validarSiTallaEsValida = talla != null;
                             if (validarSiTallaEsValida)
                             {
                                 using (AVentasEntities context = new AVentasEntities())
                                 {
                                     int productoId = 0; int tallaId = 0;

                                     var producto = coleccion.ProductosxColeccion
                                          .FirstOrDefault(x => x.CodigoProducto == talla.PRODUCT);
                                     productoId = (producto == null) ? 0 : producto.IdProducto;

                                     var grupoTalla = context.TallasXGrupo
                                          .FirstOrDefault(x => x.CodigoGrupoTalla == talla.SIZEGROUP &&
                                          x.CodigoTalla == talla.SIZE);
                                     tallaId = (grupoTalla == null) ? 0 : grupoTalla.IdTallaxGrupo;
                                     var validarData = tallaId != 0 && productoId != 0;
                                     if (validarData)
                                     {
                                         TallasxProducto tallaxProducto = new TallasxProducto()
                                         {
                                             IdProducto = productoId,
                                             IdTallaxGrupo = tallaId
                                         };
                                         if (!tallasxProductosXResult.Any(tallXProd => tallaxProducto.IdProducto == tallXProd.IdProducto && tallaxProducto.IdTallaxProducto == tallXProd.IdTallaxProducto))
                                         {
                                             tallasxProductosXResult.Add(tallaxProducto);
                                         }

                                     }
                                     else
                                     {
                                         Debug.WriteLine(JsonConvert.SerializeObject(talla));
                                     }
                                 }
                             }
                             lock (tallasxProductos)
                             {
                                 tallasxProductos.AddRange(tallasxProductosXResult);
                             }
                         });
                    }
                }
            }
            return tallasxProductos;
        }

        public async Task GuardarTallasXPRoducto(List<TallasxProducto> tallasAGuardar)
        {
            try
            {


                using (AVentasEntities context = new AVentasEntities())
                {

                    var connectionString = "data source=209.126.64.158,49170;initial catalog=Aventas;persist security info=True;user id=developer;password=D3vCitHn.20!8;MultipleActiveResultSets=True;App=EntityFramework&quot;";



                    var transaction = context.Database.BeginTransaction();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.BulkInsert("tallasxproducto", tallasAGuardar.Select(tall => new TallasxProductoModel
                        {
                            IdTallaxGrupo = tall.IdTallaxGrupo,
                            IdProducto = tall.IdProducto,

                        }).ToList());
                    }
                    transaction.Commit();

                }
            }
            catch (Exception e)
            {

                Debug.WriteLine(e);
            }
        }
    }
    public class TallasxProductoModel
    {
        public int IdTallaxProducto { get; set; }
        public Nullable<int> IdProducto { get; set; }
        public Nullable<int> IdTallaxGrupo { get; set; }

    }
}