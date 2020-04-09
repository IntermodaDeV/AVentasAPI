using AventasApi.Utils;
using DBData.Database;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorSizesByProduct
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task<List<TallaXProductoCRMApiModel>> ObtenerAtributosDesdeCRMAPI(string CodigoCollecion)
        {
            List<TallasxProducto> tallasxProductosXResult = new List<TallasxProducto>();
            var tallas = new List<TallaXProductoCRMApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoCollecion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    tallas = JsonConvert.DeserializeObject<List<TallaXProductoCRMApiModel>>(response.Content);
                
                }
            });

            if (logicValidation.ValidateDataCount(tallas.Count))
            {
                var listaTallas = tallas.GroupBy(x => new { x.PRODUCT }).Select(g => g.First()).ToList();
                return listaTallas;
            }
            return tallas;
        }

        //    public async Task<List<TallasxProducto>> ObtenerTallasXProducto()
        //    {
        //        Random randomColorNumber = new Random();
        //        List<Colecciones> colecciones = new List<Colecciones>();
        //        List<TallasxProducto> tallasxProductos = new List<TallasxProducto>();
        //        using (AVentasEntities context = new AVentasEntities())
        //        {
        //            colecciones = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().ToList();
        //        }

        //        //List<Task> taskColecciones = colecciones.ForEach(coleccion =>
        //        foreach (var coleccion in colecciones)
        //        {
        //            string peticion = string.Format(UrlString, coleccion.CodigoColeccion);
        //            var restClient = new RestClient(peticion);
        //            var request = new RestRequest(Method.GET);
        //            request.AddHeader("Accept", "application/json");
        //            IRestResponse response = restClient.Execute(request);

        //            if (response.IsSuccessful)
        //            {
        //                List<TallaXProductoCRMApiModel> tallas = JsonConvert.DeserializeObject<List<TallaXProductoCRMApiModel>>(response.Content);
        //                var validarSiTallaEsValida = tallas != null && tallas.Count > 0;
        //                if (validarSiTallaEsValida)
        //                {
        //                    List<TallasxProducto> tallasxProductosXResult = new List<TallasxProducto>();
        //                    //var result = tallas.GroupBy(x => new { x.PRODUCT }).Select(g => g.First());
        //                    Parallel.ForEach(tallas, talla =>
        //                     {
        //                         validarSiTallaEsValida = talla != null;
        //                         if (validarSiTallaEsValida)
        //                         {
        //                             using (AVentasEntities context = new AVentasEntities())
        //                             {
        //                                 int productoId = 0; int tallaId = 0;

        //                                 var producto = coleccion.ProductosxColeccion
        //                                      .FirstOrDefault(x => x.CodigoProducto == talla.PRODUCT);
        //                                 productoId = (producto == null) ? 0 : producto.IdProducto;

        //                                 var grupoTalla = context.TallasXGrupo
        //                                      .FirstOrDefault(x => x.CodigoGrupoTalla == talla.SIZEGROUP &&
        //                                      x.CodigoTalla == talla.SIZE);
        //                                 tallaId = (grupoTalla == null) ? 0 : grupoTalla.IdTallaxGrupo;
        //                                 var validarData = tallaId != 0 && productoId != 0;
        //                                 if (validarData)
        //                                 {

        //                                     lock (tallasxProductosXResult)
        //                                     {
        //                                         if (!(tallasxProductosXResult.Any(tallXProd => (tallaxProducto.IdProducto == tallXProd.IdProducto) && (tallaxProducto.IdTallaxGrupo == tallXProd.IdTallaxGrupo))))
        //                                         {
        //                                             tallasxProductosXResult.Add(tallaxProducto);
        //                                         }
        //                                     }

        //                                 }
        //                                 else
        //                                 {
        //                                     Debug.WriteLine(JsonConvert.SerializeObject(talla));
        //                                 }
        //                             }
        //                         }
        //                     });
        //                    tallasxProductos.AddRange(tallasxProductosXResult);
        //                }
        //            }
        //        }
        //        return tallasxProductos;
        //    }

        //    public async Task GuardarTallasXPRoducto(List<TallasxProducto> tallasAGuardar)
        //    {
        //        try
        //        {


        //            using (AVentasEntities context = new AVentasEntities())
        //            {

        //                var connectionString = "data source=209.126.64.158,49170;initial catalog=Aventas;persist security info=True;user id=developer;password=D3vCitHn.20!8;MultipleActiveResultSets=True;App=EntityFramework&quot;";



        //                var transaction = context.Database.BeginTransaction();
        //                using (var connection = new SqlConnection(connectionString))
        //                {
        //                    connection.BulkInsert("tallasxproducto", tallasAGuardar.Select(tall => new TallasxProductoModel
        //                    {
        //                        IdTallaxGrupo = tall.IdTallaxGrupo,
        //                        IdProducto = tall.IdProducto,

        //                    }).ToList());
        //                }
        //                transaction.Commit();

        //            }
        //        }
        //        catch (Exception e)
        //        {

        //            Debug.WriteLine(e);
        //        }
        //    }
        //}
        //public class TallasxProductoModel
        //{
        //    public int IdTallaxProducto { get; set; }
        //    public Nullable<int> IdProducto { get; set; }
        //    public Nullable<int> IdTallaxGrupo { get; set; }

    }
}