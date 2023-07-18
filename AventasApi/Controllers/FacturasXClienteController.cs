using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
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

        [HttpGet]
        [Route("descuentovencido/{cliente}")]
        public async Task<IHttpActionResult> ObtenerFacturasDescuentoVencido(string cliente)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var facturasDescuentoVencido = await ctx.FacturasxCliente
                        .Where(x=>x.CodigoCliente == cliente && x.Saldo>0 && x.FechaMaxDescuento < DateTime.Now)
                        .Select(x => new { documento=x.Tipo ,numero = x.Factura,fecha=x.FechaFactura,vencimiento=x.FechaVencimiento ,valor=x.TotalFactura,saldo=x.Saldo,excepcionDescuento = x.ExcepcionDescuento })
                        .ToListAsync();

                    return Ok(facturasDescuentoVencido);
                }
            }catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
