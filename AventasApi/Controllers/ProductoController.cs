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

        [HttpPost]
        [Route("actualizarPrioridadProducto/{codigo}/{coleccion}/{prioridad}")]
        public async Task<IHttpActionResult> actualizarPrioridadProducto(string codigo, string coleccion, int prioridad)
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
                        producto.Prioridad = prioridad;
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

        [HttpPost]
        [Route("actualizarPrioridadColorProducto/{IdColorxProducto}/{prioridad}")]
        public async Task<IHttpActionResult> ActualizarPrioridadColorProducto(int IdColorxProducto, int prioridad)
        {
            try
            {

                using (AVentasEntities context = new AVentasEntities())
                {
                    ColoresxProducto productosBD = await context.ColoresxProducto.Where(x => x.IdColorxProducto == IdColorxProducto)
                        .FirstOrDefaultAsync();

                    if(productosBD != null)
                    {
                        productosBD.Prioridad = prioridad;
                    }

                     await context.SaveChangesAsync();
                   

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("activasDeshabilitarColorPrducto/{IdColorxProducto}")]
        public async Task<IHttpActionResult> ActivasDeshabilitarColorPrducto(int IdColorxProducto)
        {
            try
            {

                using (AVentasEntities context = new AVentasEntities())
                {
                    var colorBD = await context.ColoresxProducto.FirstOrDefaultAsync(a => a.IdColorxProducto == IdColorxProducto);

                    if(colorBD != null)
                    {
                        colorBD.Deshabilitado = !colorBD.Deshabilitado;
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
        [Route("actualizarNuevoProducto/{codigo}/{coleccion}")]
        public async Task<IHttpActionResult> ActualizarNuevoProducto(string codigo, string coleccion)
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
                        producto.Nuevo = !producto.Nuevo;
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
