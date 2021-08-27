using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/productodevolucion")]
    public class ProductosDevolucionController : ApiController
    {
        [HttpGet]
        [Route("factura/{factura}")]
        public IHttpActionResult GetProductosFactura(string factura)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var productos = ctx.PRODUCTOSDEFACTURA(factura).ToList();
                    return Ok(productos);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("producto/{id}")]
        public async Task<IHttpActionResult> GetProducto(int id, [FromUri] List<string> colores, [FromUri] List<string> tallas)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var prod = ctx.ProductosxColeccion.Where(pxc => pxc.IdProducto == id).Select(pxc => new ProductoXColeccionViewModel
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
                        ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto).Select(txp => txp.TallasXGrupo).Where(t=>tallas.Contains(t.CodigoTalla))
                                             .Select(txp => new TallaViewModel
                                             {
                                                 Talla = txp.CodigoTalla.ToUpper(),
                                                 GrupoTallaId = txp.CodigoGrupoTalla,
                                                 Orden = txp.Orden ?? 0,
                                             }).OrderBy(txp => txp.Orden).ToList(),
                        ListaColores = pxc.ColoresxProducto.Where(x => colores.Contains(x.CodigoColor)).Select(cpp => new ColorViewModel
                        {
                            CodigoColor = cpp.Colores.CodigoColor,
                            NombreColor = cpp.Colores.Color,
                            Color = cpp.Colores.Rgb,
                        }).ToList(),
                    }).ToList();

                    return Ok(prod.First());
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
