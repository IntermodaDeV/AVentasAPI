using AventasApi.Models.ViewModels;
using DBData.Database;
using ExternalApiData.Enviroments;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class ProductoBarra
    {
        public string productoId { get; set; }
        public string colorId { get; set; }
        public string tallaId { get; set; }
    }
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
        [Route("factura/{factura}/{producto}/{color}")]
        public IHttpActionResult GetProductoFactura(string factura,string producto,string color)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var productos = ctx.PRODUCTOSDEFACTURA(factura).Where(x=>x.CodigoProducto == producto && x.CodigoColor.ToUpper() == color.ToUpper()).ToList();
                    return Ok(productos);
                }
            }
            catch (Exception e)
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
                        ListaTalla = ctx.GrupoTalla.FirstOrDefault(x=>x.CodigoGrupoTalla == pxc.CodigoGrupoTalla).TallasXGrupo.Where(t => tallas.Contains(t.CodigoTalla)).Select(txp => new TallaViewModel
                        {
                            Talla = txp.CodigoTalla.ToUpper(),
                            GrupoTallaId = txp.CodigoGrupoTalla,
                            Orden = txp.Orden ?? 0,
                        }).OrderBy(txp => txp.Orden).ToList(),
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
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("codigobarra/{codigo}")]
        public IHttpActionResult GetProductoBarra(string codigo)
        {
            try
            {
                var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}productos/imhn/{codigo}/codigobarra");
                client.Timeout = 480 * (1000);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                var response = client.Execute<List<ProductoBarra>>(request);

                if (response.Data.Count == 0)
                {
                    return NotFound();
                }

                return Ok(response.Data[0]);
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
