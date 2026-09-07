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
    public class ConfiguracionCorreoController : ApiController
    {
        // Roles activos asignados al usuario. Se usa para restringir a que Tipos de
        // Configuracion de Correo tiene acceso (TipoConfiguracionCorreoRol).
        private IQueryable<int> RolesActivosDeUsuario(AVentasEntities ctx, string usuario)
        {
            return ctx.Usuario_Rol
                .Where(ur => ur.status == true && ur.Roles.Status == true && ur.Usuarios.usuario == usuario)
                .Select(ur => ur.rolId);
        }

        [HttpGet]
        [Route("~/api/configuracioncorreo/tipospermitidos/{usuario}")]
        public async Task<IHttpActionResult> ObtenerTiposPermitidos(string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolesUsuario = RolesActivosDeUsuario(ctx, usuario);

                    var tipos = await ctx.TipoConfiguracionCorreoRol
                        .Where(x => rolesUsuario.Contains(x.RolId) && x.TipoConfiguracionCorreo.Activo == true)
                        .Select(x => new { x.TipoConfiguracionCorreo.Id, x.TipoConfiguracionCorreo.Codigo, x.TipoConfiguracionCorreo.Descripcion })
                        .Distinct()
                        .ToListAsync();

                    return Ok(tipos);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/configuracioncorreo/listar/{usuario}")]
        public async Task<IHttpActionResult> Listar(string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolesUsuario = RolesActivosDeUsuario(ctx, usuario);
                    var tiposPermitidos = ctx.TipoConfiguracionCorreoRol
                        .Where(x => rolesUsuario.Contains(x.RolId))
                        .Select(x => x.TipoConfiguracionCorreoId);

                    var lista = await ctx.ConfiguracionCorreo
                        .Where(c => tiposPermitidos.Contains(c.TipoConfiguracionId))
                        .Select(c => new
                        {
                            c.Id,
                            c.EmpresaId,
                            NombreEmpresa = c.Empresa.NombreEmpresa,
                            c.TipoConfiguracionId,
                            TipoDescripcion = c.TipoConfiguracionCorreo.Descripcion,
                            c.CorreosDestino,
                            c.CorreosCopia,
                            c.Asunto,
                            c.CuerpoPlantilla,
                            c.Activo,
                            c.FechaCreacion,
                            c.UsuarioCreacion,
                            c.UsuarioModifico,
                            c.FechaModificacion
                        })
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
        [Route("~/api/configuracioncorreo/crear")]
        public async Task<IHttpActionResult> Crear([FromBody] ConfiguracionCorreoModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolesUsuario = RolesActivosDeUsuario(ctx, model.Usuario);
                    var permitido = await ctx.TipoConfiguracionCorreoRol
                        .AnyAsync(x => x.TipoConfiguracionCorreoId == model.TipoConfiguracionId && rolesUsuario.Contains(x.RolId));

                    if (!permitido)
                    {
                        return BadRequest("No tiene permiso para configurar correos de este tipo.");
                    }

                    var nuevo = new ConfiguracionCorreo()
                    {
                        EmpresaId = model.EmpresaId,
                        TipoConfiguracionId = model.TipoConfiguracionId,
                        CorreosDestino = model.CorreosDestino,
                        CorreosCopia = model.CorreosCopia,
                        Asunto = model.Asunto,
                        CuerpoPlantilla = model.CuerpoPlantilla,
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = model.Usuario
                    };

                    ctx.ConfiguracionCorreo.Add(nuevo);
                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return BadRequest("Ya existe una configuracion de correo para esta empresa y este tipo.");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPut]
        [Route("~/api/configuracioncorreo/modificar")]
        public async Task<IHttpActionResult> Modificar([FromBody] ConfiguracionCorreoModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var config = await ctx.ConfiguracionCorreo.FindAsync(model.Id);

                    if (config == null)
                    {
                        return BadRequest("No se encuentra la configuracion de correo.");
                    }

                    var rolesUsuario = RolesActivosDeUsuario(ctx, model.Usuario);
                    var permitido = await ctx.TipoConfiguracionCorreoRol
                        .AnyAsync(x => x.TipoConfiguracionCorreoId == model.TipoConfiguracionId && rolesUsuario.Contains(x.RolId));

                    if (!permitido)
                    {
                        return BadRequest("No tiene permiso para configurar correos de este tipo.");
                    }

                    config.EmpresaId = model.EmpresaId;
                    config.TipoConfiguracionId = model.TipoConfiguracionId;
                    config.CorreosDestino = model.CorreosDestino;
                    config.CorreosCopia = model.CorreosCopia;
                    config.Asunto = model.Asunto;
                    config.CuerpoPlantilla = model.CuerpoPlantilla;
                    config.UsuarioModifico = model.Usuario;
                    config.FechaModificacion = DateTime.Now;

                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return BadRequest("Ya existe una configuracion de correo para esta empresa y este tipo.");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/configuracioncorreo/estado/{id}/{usuario}")]
        public async Task<IHttpActionResult> ModificarEstado(int id, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var config = await ctx.ConfiguracionCorreo.FindAsync(id);

                    if (config == null)
                    {
                        return BadRequest("No se encuentra la configuracion de correo.");
                    }

                    var rolesUsuario = RolesActivosDeUsuario(ctx, usuario);
                    var permitido = await ctx.TipoConfiguracionCorreoRol
                        .AnyAsync(x => x.TipoConfiguracionCorreoId == config.TipoConfiguracionId && rolesUsuario.Contains(x.RolId));

                    if (!permitido)
                    {
                        return BadRequest("No tiene permiso para modificar configuraciones de este tipo.");
                    }

                    config.Activo = !config.Activo;
                    config.UsuarioModifico = usuario;
                    config.FechaModificacion = DateTime.Now;

                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
