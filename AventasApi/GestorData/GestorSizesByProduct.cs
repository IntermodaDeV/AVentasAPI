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

namespace AventasApi.GestorData
{
    public class GestorSizesByProduct
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";
        public static bool tallasAgregadas = false;
        public static bool errorAlAgregar = false;

        public static async Task ObtenerTallasXProducto()
        {
            try
            {
                Random randomColorNumber = new Random();
                List<Colecciones> colecciones = new List<Colecciones>();
                using (AVentasEntities context = new AVentasEntities())
                {
                    colecciones = context.Colecciones.ToList();
                }

                List<Task> taskColecciones = colecciones.Select(coleccion =>
                    Task.Run(async () =>
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
                                var result = tallas.GroupBy(x => new { x.PRODUCT }).Select(g => g.First());
                                int count = result.Count();

                                foreach (var talla in result)
                                {
                                    validarSiTallaEsValida = talla != null;
                                    if (validarSiTallaEsValida)
                                    {
                                        using (AVentasEntities context = new AVentasEntities())
                                        {
                                            int productoId = 0; int tallaId = 0;
                                            var producto = context.ProductosxColeccion
                                                 .FirstOrDefault(x => x.CodigoProducto == (talla.PRODUCT ?? " "));
                                            productoId = (producto == null) ? 0 : producto.IdProducto;

                                            var grupoTalla = context.TallasXGrupo
                                                 .FirstOrDefault(x => x.CodigoGrupoTalla == (talla.SIZEGROUP ?? " ") &&
                                                 x.CodigoTalla == (talla.SIZE ?? " "));
                                            tallaId = (grupoTalla == null) ? 0 : grupoTalla.IdTallaxGrupo;


                                            var validarData = tallaId != 0 && productoId != 0;
                                            if (validarData)
                                            {
                                                TallasxProducto tallasxProducto = new TallasxProducto()
                                                {
                                                    IdProducto = productoId,
                                                    IdTallaxGrupo = tallaId
                                                };
                                                context.TallasxProducto.Add(tallasxProducto);

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
                        } })
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