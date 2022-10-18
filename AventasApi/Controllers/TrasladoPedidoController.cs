using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/trasladopedido")]
    public class TrasladoPedidoController : ApiController
    {
        [HttpGet]
        [Route("obtenerproductos/{rma}")]
        public async Task<IHttpActionResult> ObtenerProductos(string rma)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var existe = await ctx.Devolucion.FirstOrDefaultAsync(x => x.NumeroRMA == rma && x.PedidoOrigen != "" && x.FacturaOrigen != "");
                    if (existe == null)
                    {
                        return BadRequest("El RMA no existe o no pertenece a una devolucion completa");
                    }

                    var productosXRMA = ctx.SP_PRODUCTOSENBASEAL_MRA(rma).ToList();
                    return Ok(productosXRMA);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

    }
}
