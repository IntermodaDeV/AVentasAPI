using AventasApi.Models.ViewModels;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/trasladopedido")]
    public class TrasladoPedidoController : ApiController
    {
        [HttpGet]
        [Route("obtenerproductos/{rma}/{pais}")]
        public async Task<IHttpActionResult> ObtenerProductos(string rma, string pais)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var devolucion = await ctx.Devolucion.FirstOrDefaultAsync(x => x.NumeroRMA == rma && x.PedidoOrigen != "" && x.FacturaOrigen != "" && x.EmpresaId == pais);
                    if (devolucion == null)
                    {
                        return BadRequest("El RMA no existe o no pertenece a una devolucion completa");
                    }

                    var pedido = await ctx.PedidosxCliente.FirstOrDefaultAsync(x=>x.NumeroPedido == devolucion.PedidoOrigen && x.EmpresaId == pais);
                    if (pedido == null)
                    {
                        return BadRequest("El pedido origen no existe.");
                    }

                    var coleccion = await ctx.Colecciones.FirstOrDefaultAsync(x => x.IdColeccion == pedido.IdColeccion);
                    var productos = ctx.SP_ObtenerProductosRMA(rma, pais).ToList();
                    return Ok(new { coleccionId=coleccion.IdColeccion,ventaFinal=coleccion.VentaFinal,productos=productos});
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        [HttpGet]
        [Route("getProducto/{codigoProducto}/{grupoPrecio}/{coleccionId}")]
        public async Task<IHttpActionResult> GetProductos( string codigoProducto, string grupoPrecio, int coleccionId,[FromUri] List<string> colores, [FromUri] List<string> tallas)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {

                    var prod = ctx.ProductosxColeccion.Where(pxc => pxc.CodigoProducto == codigoProducto && pxc.IdColeccion == coleccionId).Select(pxc => new ProductoXColeccionViewModel
                    {
                        ProductoId = pxc.CodigoProducto,
                        idColeccion = pxc.IdColeccion,
                        CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
                        CodigoProducto = pxc.IdProducto,
                        NombreProducto = pxc.NombreProducto,
                        StockVisible = pxc.StockVisible,
                        GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                        GrupoTalla = pxc.CodigoGrupoTalla,
                        Linea = new LineaViewModel
                        {
                            IdLinea = pxc.MaestroLinea.IdLinea,
                            Linea = pxc.MaestroLinea.Linea,
                        },
                        ListaTalla = ctx.GrupoTalla.FirstOrDefault(x => x.CodigoGrupoTalla == pxc.CodigoGrupoTalla).TallasXGrupo.Where(t => tallas.Contains(t.CodigoTalla)).Select(txp => new TallaViewModel
                        {
                            Talla = txp.CodigoTalla.ToUpper(),
                            GrupoTallaId = txp.CodigoGrupoTalla,
                            Orden = txp.Orden ?? 0,
                        }).OrderBy(txp => txp.Orden).ToList(),
                        fisicaDisponible = pxc.FisicoDisponible
                          .Select(f => new FisicoDisponibleViewModel
                             {
                                 CodigoColor = f.CodigoColor,
                                 IdTalla = f.CodigoTalla.ToUpper(),
                                 Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                                 MinStock = f.MinStock,
                                 PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => true).Where(preEsp => preEsp.GrupoPrecio == grupoPrecio).Select(preEsp => new PrecioEspecificoViewModel
                                 {
                                     IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                     IdMoneda = preEsp.IdMoneda,
                                     IdProducto = preEsp.IdProducto,
                                     GrupoPrecio = preEsp.GrupoPrecio,
                                     IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                     Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : pxc.PreciosxProducto.FirstOrDefault(pre => pre.GrupoPrecio == grupoPrecio && pre.IdProducto == preEsp.IdProducto).Precio,
                                 }).ToList(),
                             }).ToList(),
                        ListaColores = ctx.Colores.Where(x => colores.Contains(x.CodigoColor)).Select(cpp => new ColorViewModel
                        {
                            CodigoColor = cpp.CodigoColor,
                            NombreColor = cpp.Color,
                            Color = cpp.Rgb,
                        }).ToList(),

                    }).ToList();

                    return Ok(prod.First());
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }






    }
}
