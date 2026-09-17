using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using DBData.Database;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class ConfiguracionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var configuraciones = context.Configuraciones.ToDictionary(conf => conf.CodigoConfiguracion,
                    conf => conf.Valor
                );
                return Ok(configuraciones);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/configuraciones/conexion")]
        public IHttpActionResult VerificarConexion()
        {
            return Ok();
        }

        // Solo usuarios con el rol "Administrador de Sistema" pueden administrar
        // (crear, editar, activar/desactivar) las configuraciones del sistema.
        private async Task<bool> EsAdministradorDeSistema(AVentasEntities ctx, string usuario)
        {
            return await ctx.Usuario_Rol
                .AnyAsync(ur => ur.status == true && ur.Roles.Status == true && ur.Usuarios.usuario == usuario && ur.Roles.Nombre == "Administrador de Sistema");
        }

        [HttpGet]
        [Route("~/api/configuraciones/listar/{usuario}")]
        public async Task<IHttpActionResult> Listar(string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    if (!await EsAdministradorDeSistema(ctx, usuario))
                    {
                        return BadRequest("No tiene permisos de Administrador de Sistema para realizar esta accion.");
                    }

                    var lista = await ctx.Configuraciones
                        .OrderBy(c => c.CodigoConfiguracion)
                        .Select(c => new
                        {
                            c.IdConfiguracion,
                            c.CodigoConfiguracion,
                            c.NombreConfiguracion,
                            c.DescripcionConfiguracion,
                            c.Valor,
                            c.Activo,
                            c.FechaCreacion,
                            c.UsuarioCreacion,
                            c.FechaModificacion,
                            c.UsuarioModificacion
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
        [Route("~/api/configuraciones/crear")]
        public async Task<IHttpActionResult> Crear([FromBody] ConfiguracionModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    if (!await EsAdministradorDeSistema(ctx, model.Usuario))
                    {
                        return BadRequest("No tiene permisos de Administrador de Sistema para realizar esta accion.");
                    }

                    var existe = await ctx.Configuraciones.AnyAsync(c => c.CodigoConfiguracion == model.CodigoConfiguracion);
                    if (existe)
                    {
                        return BadRequest("Ya existe una configuracion con ese codigo.");
                    }

                    var nueva = new Configuraciones()
                    {
                        CodigoConfiguracion = model.CodigoConfiguracion,
                        NombreConfiguracion = model.NombreConfiguracion,
                        DescripcionConfiguracion = model.DescripcionConfiguracion,
                        Valor = model.Valor,
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = model.Usuario
                    };

                    ctx.Configuraciones.Add(nueva);
                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return BadRequest("Ya existe una configuracion con ese codigo.");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPut]
        [Route("~/api/configuraciones/modificar")]
        public async Task<IHttpActionResult> Modificar([FromBody] ConfiguracionModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    if (!await EsAdministradorDeSistema(ctx, model.Usuario))
                    {
                        return BadRequest("No tiene permisos de Administrador de Sistema para realizar esta accion.");
                    }

                    var config = await ctx.Configuraciones.FindAsync(model.IdConfiguracion);
                    if (config == null)
                    {
                        return BadRequest("No se encuentra la configuracion.");
                    }

                    var codigoDuplicado = await ctx.Configuraciones
                        .AnyAsync(c => c.CodigoConfiguracion == model.CodigoConfiguracion && c.IdConfiguracion != model.IdConfiguracion);
                    if (codigoDuplicado)
                    {
                        return BadRequest("Ya existe una configuracion con ese codigo.");
                    }

                    config.CodigoConfiguracion = model.CodigoConfiguracion;
                    config.NombreConfiguracion = model.NombreConfiguracion;
                    config.DescripcionConfiguracion = model.DescripcionConfiguracion;
                    config.Valor = model.Valor;
                    config.UsuarioModificacion = model.Usuario;
                    config.FechaModificacion = DateTime.Now;

                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return BadRequest("Ya existe una configuracion con ese codigo.");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/configuraciones/estado/{id}/{usuario}")]
        public async Task<IHttpActionResult> ModificarEstado(int id, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    if (!await EsAdministradorDeSistema(ctx, usuario))
                    {
                        return BadRequest("No tiene permisos de Administrador de Sistema para realizar esta accion.");
                    }

                    var config = await ctx.Configuraciones.FindAsync(id);
                    if (config == null)
                    {
                        return BadRequest("No se encuentra la configuracion.");
                    }

                    config.Activo = !config.Activo;
                    config.UsuarioModificacion = usuario;
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
