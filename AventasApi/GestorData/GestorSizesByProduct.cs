using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.GestorData
{
    public class GestorSizesByProduct
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";

        public async Task<bool> ObtenerTallasXProducto(string colleccionId)
        {
            string peticion = string.Format(UrlString, colleccionId);
            var restClient = new RestClient(peticion);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                List<TallaPorProductoViewModel> tallas = JsonConvert.DeserializeObject<List<TallaPorProductoViewModel>>(response.Content);
                var validarSiTallaEsValida = tallas != null && tallas.Count > 0;
                if (validarSiTallaEsValida)
                {
                    var result = tallas.GroupBy(x => new { x.CodProducto }).Select(g => g.First());
                    int count = result.Count();

                    foreach (var talla in result)
                    {
                        validarSiTallaEsValida = talla != null;
                        if (validarSiTallaEsValida)
                        {
                            using (AVentasEntities context = new AVentasEntities())
                            {
                                int productoId = 0; int tallaId = 0;
                                await Task.Run(() =>
                                {
                                    var producto = context.ProductosxColeccion
                                         .FirstOrDefault(x => x.CodigoProducto == (talla.CodProducto ?? " "));
                                    productoId = (producto == null) ? 0 : producto.IdProducto;
                                });

                                await Task.Run(() =>
                                {
                                    var grupoTalla = context.TallasXGrupo
                                         .FirstOrDefault(x => x.CodigoGrupoTalla == (talla.CodTallaGrupo ?? " ") &&
                                         x.CodigoTalla == (talla.CodTalla ?? " "));
                                    tallaId = (grupoTalla == null) ? 0 : grupoTalla.IdTallaxGrupo;
                                });

                                var validarData = tallaId != 0 && productoId != 0;
                                if (validarData)
                                {
                                    TallasxProducto tallasxProducto = new TallasxProducto()
                                    {
                                        IdProducto =  productoId,
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
                    return true;
                }
            }

            return false;
        }
    }
}