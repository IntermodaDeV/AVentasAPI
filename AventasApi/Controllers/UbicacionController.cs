using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/ubicacion")]
    public class UbicacionController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> ObtenerUbicaciones()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var ubicaciones = await ctx.UbicacionesXAlmacen.Include(x=>x.MaestroBodegaAlmacenes).Select(x => new { 
                        UbicacionId=x.UbicacionId,
                        CodigoUbicacion=x.CodigoUbicacion,
                        Almacen=x.MaestroBodegaAlmacenes.Almacen,
                        Etiqueta=x.MaestroBodegaAlmacenes.Etiqueta,
                        Empresa=x.MaestroBodegaAlmacenes.EmpresaId,
                        Estatus=x.Estatus,
                        ActivoDevolucion=x.ActivoDevolucion
                    }).ToListAsync();
                    return Ok(ubicaciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("modificar/{usuario}/{id}")]
        public async Task<IHttpActionResult> ModificarUbicacion(string usuario, int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var ubicaciones = await ctx.UbicacionesXAlmacen.FindAsync(id);

                    if (ubicaciones == null)
                    {
                        return BadRequest("El sitio no existe");
                    }

                    ubicaciones.Estatus = !ubicaciones.Estatus;
                    ubicaciones.ModificadoPor = usuario;
                    ubicaciones.FechaModificacion = DateTime.Now;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
        
        [HttpPost]
        [Route("modificarEstadoUbicacion/{usuario}/{id}")]
        public async Task<IHttpActionResult> ModificarEstadoUbicacion(string usuario, int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var ubicaciones = await ctx.UbicacionesXAlmacen.FindAsync(id);

                    if (ubicaciones == null)
                    {
                        return BadRequest("El sitio no existe");
                    }

                    ubicaciones.ActivoDevolucion = !ubicaciones.ActivoDevolucion;
                    ubicaciones.ModificadoPor = usuario;
                    ubicaciones.FechaModificacion = DateTime.Now;
                    await ctx.SaveChangesAsync();

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
