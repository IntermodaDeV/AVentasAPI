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
    public class TipoConfiguracionCorreoController : ApiController
    {
        [HttpGet]
        [Route("~/api/tipoconfiguracioncorreo")]
        public async Task<IHttpActionResult> ObtenerTipos()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var lista = await ctx.TipoConfiguracionCorreo
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

        [HttpPost]
        [Route("~/api/tipoconfiguracioncorreo/crear")]
        public async Task<IHttpActionResult> CrearTipo([FromBody] TipoConfiguracionCorreoModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var nuevo = new TipoConfiguracionCorreo()
                    {
                        Codigo = model.Codigo,
                        Descripcion = model.Descripcion,
                        Activo = model.Activo
                    };

                    ctx.TipoConfiguracionCorreo.Add(nuevo);
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
        [Route("~/api/tipoconfiguracioncorreo/modificar")]
        public async Task<IHttpActionResult> ModificarTipo([FromBody] TipoConfiguracionCorreoModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var tipo = await ctx.TipoConfiguracionCorreo.FindAsync(model.Id);

                    if (tipo == null)
                    {
                        return BadRequest("No se encuentra el tipo de configuracion.");
                    }

                    tipo.Codigo = model.Codigo;
                    tipo.Descripcion = model.Descripcion;
                    tipo.Activo = model.Activo;

                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/tipoconfiguracioncorreo/estado/{id}")]
        public async Task<IHttpActionResult> ModificarEstado(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var tipo = await ctx.TipoConfiguracionCorreo.FindAsync(id);

                    if (tipo == null)
                    {
                        return BadRequest("No se encuentra el tipo de configuracion.");
                    }

                    tipo.Activo = !tipo.Activo;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/tipoconfiguracioncorreo/rolesasignados/{id}")]
        public async Task<IHttpActionResult> ObtenerRolesAsignados(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var asignados = await ctx.TipoConfiguracionCorreoRol
                        .Where(x => x.TipoConfiguracionCorreoId == id)
                        .Select(x => new { x.Roles.Id, x.Roles.Nombre })
                        .ToListAsync();

                    return Ok(asignados);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/tipoconfiguracioncorreo/rolesnoasignados/{id}")]
        public async Task<IHttpActionResult> ObtenerRolesNoAsignados(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var asignadosIds = ctx.TipoConfiguracionCorreoRol
                        .Where(x => x.TipoConfiguracionCorreoId == id)
                        .Select(x => x.RolId);

                    var noAsignados = await ctx.Roles
                        .Where(r => r.Status == true && !asignadosIds.Contains(r.Id))
                        .Select(r => new { r.Id, r.Nombre })
                        .ToListAsync();

                    return Ok(noAsignados);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/tipoconfiguracioncorreo/asignarrol/{idTipo}/{idRol}")]
        public async Task<IHttpActionResult> AsignarRol(int idTipo, int idRol)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var existe = await ctx.TipoConfiguracionCorreoRol
                        .AnyAsync(x => x.TipoConfiguracionCorreoId == idTipo && x.RolId == idRol);

                    if (!existe)
                    {
                        ctx.TipoConfiguracionCorreoRol.Add(new TipoConfiguracionCorreoRol
                        {
                            TipoConfiguracionCorreoId = idTipo,
                            RolId = idRol
                        });

                        await ctx.SaveChangesAsync();
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
        [Route("~/api/tipoconfiguracioncorreo/removerrol/{idTipo}/{idRol}")]
        public async Task<IHttpActionResult> RemoverRol(int idTipo, int idRol)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var relacion = await ctx.TipoConfiguracionCorreoRol
                        .FirstOrDefaultAsync(x => x.TipoConfiguracionCorreoId == idTipo && x.RolId == idRol);

                    if (relacion == null)
                    {
                        return BadRequest("El tipo no tiene asignado ese rol.");
                    }

                    ctx.TipoConfiguracionCorreoRol.Remove(relacion);
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
