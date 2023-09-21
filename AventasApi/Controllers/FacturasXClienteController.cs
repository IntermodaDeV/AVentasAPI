using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using AventasApi.Models.ViewModels;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/factura")]
    public class FacturasXClienteController : ApiController
    {
        [HttpPut]
        [Route("excepciondescuento/{cliente}/{factura}")]
        public async Task<IHttpActionResult> ActualizarExcepcionFactura(string cliente,string factura)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    FacturasxCliente facturaBD = await ctx.FacturasxCliente.FirstOrDefaultAsync(x=>x.CodigoCliente == cliente && x.Factura == factura);
                    if (facturaBD == null)
                    {
                        return NotFound();
                    }

                    facturaBD.ExcepcionDescuento = !facturaBD.ExcepcionDescuento;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("diasgraciafactura/{cliente}/{factura}/{dias}")]
        public async Task<IHttpActionResult> ActualizarDiasdeGracia(string cliente, string factura, int dias)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    FacturasxCliente facturaBD = await ctx.FacturasxCliente.FirstOrDefaultAsync(x => x.CodigoCliente == cliente && x.Factura == factura);
                    if (facturaBD == null)
                    {
                        return NotFound();
                    }

                    facturaBD.DiasGracia = dias;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("descuentovencido/{cliente}")]
        public async Task<IHttpActionResult> ObtenerFacturasDescuentoVencido(string cliente)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var facturas = await ctx.FacturasxCliente
                        .Include(x => x.Clientes)
                        .Where(x => x.CodigoCliente == cliente && x.Saldo > 0 && x.FechaMaxDescuento < DateTime.Now && x.IdAcuerdoxCliente == null)
                        .ToListAsync();

                    foreach (var factura in facturas)
                    {
                        if (factura.Descuento > 0)
                        {
                            continue;
                        }

                        var grupoDescuento = await ctx.Descuento.FirstOrDefaultAsync(x => x.Codigo.ToUpper() == factura.Clientes.Descuento.ToUpper() && x.EmpresaId.ToUpper() == factura.Clientes.EmpresaId.ToUpper());
                        if (grupoDescuento == null)
                        {
                            continue;
                        }

                        var descuentoDetalle = await ctx.DescuentoDetalle.FirstOrDefaultAsync(x => x.IdDescuento == grupoDescuento.IdDescuento && x.IdLinea.ToUpper() == factura.IdLinea.ToUpper());
                        if (descuentoDetalle == null)
                        {
                            continue;
                        }

                        var totalDocumentosAplicados = ctx.DocumentosAplicadosAFacturas.Where(x => x.Factura == factura.Factura).Sum(x => x.Valor) ?? 0;
                        var subfactura = await ctx.SubFacturasxCliente.FirstOrDefaultAsync(x => x.Factura == factura.Factura && x.Flete != null);
                        decimal? flete = 0;

                        if (subfactura != null)
                        {
                            flete = subfactura.Flete.Value;
                        }

                        var totalFactura = factura.TotalFactura - totalDocumentosAplicados - flete;
                        var descuento = totalFactura * (descuentoDetalle.Porcentaje / 100);
                        factura.FechaMaxDescuento = factura.FechaFactura?.AddDays(descuentoDetalle.DiasDescuento.Value);
                        factura.Descuento = descuento;
                    }

                    var facturasDescuentoVencido = facturas
                        .Where(x => x.Descuento > 0 && x.FechaMaxDescuento < DateTime.Now)
                        .Select(x => new { documento = x.Tipo, numero = x.Factura, fecha = x.FechaFactura, vencimiento = x.FechaVencimiento, valor = x.TotalFactura, saldo = x.Saldo, excepcionDescuento = x.ExcepcionDescuento, descuento = x.Descuento, vencimientoDescuento = x.FechaMaxDescuento })
                        .ToList();


                    return Ok(facturasDescuentoVencido);
                }
            }catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{cliente}")]
        public async Task<IHttpActionResult> GetFacturaCliente(string cliente)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var facturas = await ctx.FacturasxCliente
                        .Where(x => x.CodigoCliente == cliente && x.Tipo.Contains("Factura") && x.NumeroPedido != null)
                        .OrderByDescending(x => x.FechaFactura)
                        .Select(x => new { factura = x.Factura, pedido = x.NumeroPedido, linea = x.IdLinea })
                        .ToListAsync();

                    return Ok(facturas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

        }

        [HttpGet]
        [Route("facturasdiasgracias/{cliente}")]
        public async Task<IHttpActionResult> GetFacturasCliente(string cliente)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var facturas = ctx.FacturasxCliente.Where(f => f.Saldo > 0.1M && f.CodigoCliente == cliente).Select(fa => new FacturasXClienteDiasGraciaViewModel
                    {
                        Tipo = fa.Tipo,
                        Factura = fa.Factura,
                        FechaFactura = fa.FechaFactura,
                        FechaVencimiento = fa.FechaVencimiento,
                        TotalFactura = fa.TotalFactura,
                        Saldo = fa.Saldo,
                        Descuento = fa.Saldo,
                        FechaMaxDescuento = fa.FechaMaxDescuento,
                        CodigoCliente = fa.CodigoCliente,
                        DiasGracia = fa.DiasGracia ?? 0
                    }).ToList();

                    return Ok(facturas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

        }
    }
}
