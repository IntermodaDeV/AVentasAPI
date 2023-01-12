using AventasApi.Models;
using AventasApi.Models.ViewModels;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;

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
                        Precio = pxc.PreciosxProducto.Where(preEsp =>/* true || */preEsp.GrupoPrecio == grupoPrecio).Select(precio => new PrecioXProductoViewModel
                        {
                            GrupoPrecio = precio.GrupoPrecio,
                            IdMoneda = precio.IdMoneda,
                            Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                        }).ToList(),
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
                            Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                            {
                                IdDistribucion = dis.IdDistribucion,
                                IdTallaxGrupo = dis.IdTallaxGrupo,
                                NombreDistribucion = dis.NombreDistribucion,
                                NombreTalla = dis.NombreTalla,
                                Cantidad = dis.Cantidad,
                                Orden = dis.Orden
                            }).ToList(),
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


        [HttpGet]
        [Route("getcoleccioneById/{coleccionId}")]
        public async Task<IHttpActionResult> GetcoleccioneById(int coleccionId)
        {
            try
            {

                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var coleccion = await ctx.Colecciones.FirstOrDefaultAsync(a => a.IdColeccion == coleccionId);

                    if (coleccion == null)
                    {
                        return NotFound();
                    }

                    var coleccionDTO = new
                    {
                        id = coleccion.IdColeccion,
                        codigoColeccion = coleccion.CodigoColeccion,
                        ventaFinal = coleccion.VentaFinal,
                        entregaInicio = coleccion.EntregaInicio,
                        entregaFinal = coleccion.EntregaFinal,
                        linea = coleccion.Linea,
                        coleccionTipo = coleccion.ColeccionTipo
                    };

                    return Ok(coleccionDTO);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("getPedido/{pedidoId}")]
        public async Task<IHttpActionResult> GetPedido(string pedidoId)
        {
            try
            {

                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var firma = "data:image/png;base64," + Convert.ToBase64String(ctx.FirmasxPedido.FirstOrDefault(fir => fir.PedidoId == pedidoId).Firma);
                    List<PedidosXClienteViewModel> pedidos = ctx.PedidosxCliente.Where(p => p.PedidoId == pedidoId).OrderByDescending(ped => ped.PedidoId).Select(ped => new PedidosXClienteViewModel
                    {
                        Asesor = ped.CodigoAsesor,
                        PedidoId = ped.PedidoId,
                        BodegaEspecifica = ped.BodegaEspecifica,
                        NumeroPedido = ped.NumeroPedido,
                        Sincronizado = ped.Sincronizado,
                        CodigoColeccion = ped.Colecciones.CodigoColeccion,
                        NombreColeccion = ped.Colecciones.Nombre,
                        TotalUnidades = ped.TotalUnidades,
                        TotalXPedido = ped.TotalPedido,
                        SubTotalXPedido = ped.Subtotal,
                        Impuesto = ped.TotalImpuesto,
                        ClienteContadoId = ped.ClienteContadoId,
                        ModoVenta = ped.ModoVenta,
                        Flete = ped.Flete,
                        Firma = firma,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = ped.Clientes.CodigoCliente,
                            Nombre = ped.Clientes.Nombre,
                            Direccion = ped.Clientes.Direccion,
                            Moneda = ped.Clientes.IdMoneda,
                            EmpresaId = ped.Clientes.EmpresaId
                        },
                        Linea = ctx.MaestroLinea.Select(ml => new LineaViewModel
                        {
                            IdLinea = ml.IdLinea,
                            Linea = ml.Linea,
                        }).FirstOrDefault(ml => ml.IdLinea == ped.IdLinea),
                        TipoPedido = ctx.TiposdePedido.Select(tp => new TipoPedidoViewModel
                        {
                            IdTipoPedido = tp.IdTipoPedido,
                            TipoPedido = tp.TipoPedido,
                            HabilitaEstilos = tp.HabilitaEstilos ?? false,
                            Imagen = tp.Url_Imagen,
                            Aplica_Todos = tp.Aplica_Todos ?? false,
                            Restrictivo = tp.Restrictivo ?? false
                        }).FirstOrDefault(tp => tp.IdTipoPedido == ped.IdTipoPedido),
                        AcuerdoVenta = ped.AcuerdoVenta,
                        EmpresaId = ped.EmpresaId,
                        FechaActual = ped.Fecha,
                        Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Nombre,
                        FechaEntrega = ped.FechaEntrega,
                        Observacion = ped.Observacion,
                        location = new Location
                        {
                            mocked = ped.Mocked ?? false,
                            accuracy = ped.Accuracy,
                            altitude = ped.Altitude,
                            latitude = ped.Latitude,
                            longitude = ped.Longitude,
                            error = ped.Error
                        },
                        locationCliente = new LocationCliente
                        {
                            latitude = ped.Clientes.Latitud,
                            longitude = ped.Clientes.Longitud
                        },
                        gruposXDetPed = ped.PedidosDetalle.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
                            .Select(gruposXDetPed => new GruposTallaXDetPed
                            {
                                GrupoTalla = gruposXDetPed.Key,
                                ListaTalla = gruposXDetPed.GroupBy(pedDet => pedDet.CodigoTalla).Select(pedDet => pedDet.Key).SelectMany(pedDet => ctx.TallasXGrupo.Where(txp => txp.CodigoTalla == pedDet && txp.CodigoGrupoTalla == gruposXDetPed.Key)).Select(txp => new TallaViewModel
                                {
                                    GrupoTallaId = txp.CodigoGrupoTalla,
                                    Talla = txp.CodigoTalla,
                                    Orden = txp.Orden ?? 0,
                                    Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                    {
                                        IdDistribucion = dis.IdDistribucion,
                                        IdTallaxGrupo = dis.IdTallaxGrupo,
                                        NombreDistribucion = dis.NombreDistribucion,
                                        NombreTalla = dis.NombreTalla,
                                        Cantidad = dis.Cantidad,
                                        Orden = dis.Orden
                                    }).ToList()
                                }).OrderBy(txp => txp.Orden).ToList(),
                                prodsXDetPed = gruposXDetPed.GroupBy(pedDet => pedDet.IdProducto)
                            .Select(pedDet => new ProductosXDetPed
                            {
                                IdProducto = pedDet.Key,
                                CodigoProducto = pedDet.FirstOrDefault().ProductosxColeccion.CodigoProducto,
                                NombreProducto = pedDet.FirstOrDefault().ProductosxColeccion.NombreProducto,
                                Imagen = pedDet.FirstOrDefault().ProductosxColeccion.FotografiasXProducto.FirstOrDefault().FotografiaProducto,
                                CantidadXProducto = pedDet.Sum(cant => cant.Cantidad),
                                TotalXProducto = pedDet.Sum(cant => cant.MontoLinea),
                                coloresXProdXDetPed = pedDet.GroupBy(colXprod => colXprod.CodigoColor).Where(colXprod => colXprod.Sum(det => det.Cantidad) > 0).Select(colXprod =>
                                         new ColoresXProdXDetPed
                                         {
                                             CantidadXColor = colXprod.Sum(cant => cant.Cantidad),
                                             TotalXColor = colXprod.Sum(cant => cant.MontoLinea),
                                             PrecioXColor = colXprod.FirstOrDefault().PrecioUnitario,
                                             IdColor = colXprod.Key,
                                             NombreColor = ctx.Colores.FirstOrDefault(color => color.CodigoColor == colXprod.Key).Color,
                                             DetallesXPedido = colXprod.Select(detPed => new DetalleXPedidoViewModel
                                             {
                                                 IdRegistro = detPed.IdPedidoDetalle,
                                                 PedidoId = detPed.PedidoId,
                                                 Cantidad = detPed.Cantidad,

                                                 Linea = detPed.Linea,
                                                 MontoLinea = detPed.MontoLinea,
                                                 PrecioUnitario = detPed.PrecioUnitario,
                                                 Talla = detPed.CodigoTalla,
                                                 TallaObject = ctx.TallasXGrupo.Where(txp => txp.CodigoGrupoTalla == detPed.ProductosxColeccion.CodigoGrupoTalla && txp.CodigoTalla == detPed.CodigoTalla)/*.Where(txp => false || (ped.Colecciones.ColeccionTipo == "F") || gruposXDetPed.Any(pxc => pxc.ProductosxColeccion.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) > 0))*/.Select(txp => new TallaViewModel
                                                 {
                                                     GrupoTallaId = txp.CodigoGrupoTalla,
                                                     Talla = txp.CodigoTalla,
                                                     Orden = txp.Orden ?? 0,
                                                     Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                                     {
                                                         IdDistribucion = dis.IdDistribucion,
                                                         IdTallaxGrupo = dis.IdTallaxGrupo,
                                                         NombreDistribucion = dis.NombreDistribucion,
                                                         NombreTalla = dis.NombreTalla,
                                                         Cantidad = dis.Cantidad,
                                                         Orden = dis.Orden
                                                     }).ToList()
                                                 }).FirstOrDefault()
                                             }).ToList()

                                         }).ToList()
                            }).ToList()
                            }).ToList()
                    }).ToList();


            



                    return Ok(pedidos.First());
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }



    }
}
