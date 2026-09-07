using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class MotivoAnulacionController : ApiController
    {
        [HttpGet]
        [Route("~/api/motivoanulacion")]
        public async Task<IHttpActionResult> ObtenerMotivos()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var lista = await ctx.MotivoAnulacion
                        .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
                        .ToListAsync();

                    return Ok(lista);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/motivoanulacion/activos")]
        public async Task<IHttpActionResult> ObtenerMotivosActivos()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var lista = await ctx.MotivoAnulacion
                        .Where(x => x.Activo == true)
                        .Select(x => new { x.Id, x.Codigo, x.Descripcion })
                        .ToListAsync();

                    return Ok(lista);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/motivoanulacion/crear")]
        public async Task<IHttpActionResult> CrearMotivo([FromBody] MotivoAnulacionModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = ctx.Usuarios.FirstOrDefault(u => u.usuario == model.Usuario);

                    var nuevo = new MotivoAnulacion()
                    {
                        Codigo = model.Codigo,
                        Descripcion = model.Descripcion,
                        Activo = model.Activo,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuario?.Id
                    };

                    ctx.MotivoAnulacion.Add(nuevo);
                    await ctx.SaveChangesAsync();
                    return Ok(new { Id = nuevo.Id });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return BadRequest("El código ya existe. Por favor, elige un código diferente.");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/motivoanulacion/modificar")]
        public async Task<IHttpActionResult> ModificarMotivo([FromBody] MotivoAnulacionModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var motivo = await ctx.MotivoAnulacion.FindAsync(model.Id);

                    if (motivo == null)
                    {
                        return BadRequest("No se encuentra el motivo de anulación.");
                    }

                    var usuario = ctx.Usuarios.FirstOrDefault(u => u.usuario == model.Usuario);

                    motivo.Codigo = model.Codigo;
                    motivo.Descripcion = model.Descripcion;
                    motivo.Activo = model.Activo;
                    motivo.UsuarioModifico = usuario?.Id;
                    motivo.FechaModificacion = DateTime.Now;

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
        [Route("~/api/motivoanulacion/estado/{id}")]
        public async Task<IHttpActionResult> ModificarEstado(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var motivo = await ctx.MotivoAnulacion.FindAsync(id);

                    if (motivo == null)
                    {
                        return BadRequest("No se encuentra el motivo de anulación.");
                    }

                    motivo.Activo = !motivo.Activo;
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
