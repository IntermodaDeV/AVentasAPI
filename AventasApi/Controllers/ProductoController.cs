using DBData.Database;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{

    [RoutePrefix("api/product")]
    public class ProductoController : ApiController
    {

        [HttpPost]
        [Route("actualizarInOutProducto/{codigo}/{coleccion}")]
        public async Task<IHttpActionResult> ActualizarInOutProducto(string codigo, string coleccion)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    var productosBD = await context.ProductosxColeccion
                        .Include(x=>x.Colecciones)
                        .Where(x=>x.CodigoProducto == codigo && x.Colecciones.CodigoColeccion == coleccion)
                        .ToListAsync();

                    foreach(var producto in productosBD)
                    {
                        producto.InOut = !producto.InOut;
                    }

                    if(productosBD.Count > 0)
                    {
                        await context.SaveChangesAsync();
                    }
                    
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("activarDeshabilitarPrducto/{codigo}/{coleccion}")]
        public async Task<IHttpActionResult> ActivasDeshabilitarPrducto(string codigo, string coleccion)
        {
            try
            {

                using (AVentasEntities context = new AVentasEntities())
                {
                    var productosBD = await context.ProductosxColeccion
                    .Include(x => x.Colecciones)
                        .Where(x => x.CodigoProducto == codigo && x.Colecciones.CodigoColeccion == coleccion)
                        .ToListAsync();

                    foreach (var producto in productosBD)
                    {
                        producto.Deshabilitado = !producto.Deshabilitado;
                    }

                    if (productosBD.Count > 0)
                    {
                        await context.SaveChangesAsync();
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
