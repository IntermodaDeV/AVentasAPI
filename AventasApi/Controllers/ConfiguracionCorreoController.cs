using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using AventasApi.Utils;
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

                    var baseLista = await ctx.ConfiguracionCorreo
                        .Where(c => tiposPermitidos.Contains(c.TipoConfiguracionId))
                        .Select(c => new
                        {
                            c.Id,
                            c.EmpresaId,
                            NombreEmpresa = c.Empresa.NombreEmpresa,
                            c.TipoConfiguracionId,
                            TipoDescripcion = c.TipoConfiguracionCorreo.Descripcion,
                            c.Asunto,
                            c.CuerpoPlantilla,
                            c.Activo,
                            c.FechaCreacion,
                            c.UsuarioCreacion,
                            c.UsuarioModifico,
                            c.FechaModificacion
                        })
                        .ToListAsync();

                    var lista = new List<object>();

                    foreach (var item in baseLista)
                    {
                        var correosDestino = await ConfiguracionCorreoHelper.ObtenerCorreosAsync(ctx, item.Id, "DESTINO");
                        var correosCopia = await ConfiguracionCorreoHelper.ObtenerCorreosAsync(ctx, item.Id, "COPIA");

                        lista.Add(new
                        {
                            item.Id,
                            item.EmpresaId,
                            item.NombreEmpresa,
                            item.TipoConfiguracionId,
                            item.TipoDescripcion,
                            CorreosDestino = string.Join(", ", correosDestino),
                            CorreosCopia = string.Join(", ", correosCopia),
                            item.Asunto,
                            item.CuerpoPlantilla,
                            item.Activo,
                            item.FechaCreacion,
                            item.UsuarioCreacion,
                            item.UsuarioModifico,
                            item.FechaModificacion
                        });
                    }

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
                        // Los destinatarios ya no se guardan como texto aquí; viven en ConfiguracionCorreoUsuario.
                        // CorreosDestino sigue siendo NOT NULL en la tabla, se deja vacío sin uso.
                        CorreosDestino = "",
                        Asunto = model.Asunto,
                        CuerpoPlantilla = model.CuerpoPlantilla,
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = model.Usuario
                    };

                    ctx.ConfiguracionCorreo.Add(nuevo);
                    await ctx.SaveChangesAsync();
                    return Ok(new { success = true, id = nuevo.Id });
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

        [HttpGet]
        [Route("~/api/configuracioncorreo/destinatarios/{configuracionCorreoId}")]
        public async Task<IHttpActionResult> ObtenerDestinatarios(int configuracionCorreoId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var lista = await ctx.ConfiguracionCorreoUsuario
                        .Where(x => x.ConfiguracionCorreoId == configuracionCorreoId && x.Activo == true)
                        .Select(x => new
                        {
                            x.Id,
                            x.Tipo,
                            x.UsuarioId,
                            NombreUsuario = x.Usuarios.nombre,
                            CorreoUsuario = x.Usuarios.Correo,
                            x.CorreoManual
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
        [Route("~/api/configuracioncorreo/destinatarios/crear")]
        public async Task<IHttpActionResult> CrearDestinatario([FromBody] ConfiguracionCorreoUsuarioModel model)
        {
            try
            {
                if (model.Tipo != "DESTINO" && model.Tipo != "COPIA")
                {
                    return BadRequest("Tipo inválido, debe ser DESTINO o COPIA.");
                }

                bool tieneUsuario = model.UsuarioId != null;
                bool tieneCorreoManual = !string.IsNullOrWhiteSpace(model.CorreoManual);

                if (tieneUsuario == tieneCorreoManual)
                {
                    return BadRequest("Seleccione un usuario del sistema o escriba un correo manual (solo una de las dos opciones).");
                }

                using (var ctx = new AVentasEntities())
                {
                    var usuarioActual = await ctx.Usuarios.FirstOrDefaultAsync(u => u.usuario == model.Usuario);

                    var existente = await ctx.ConfiguracionCorreoUsuario.FirstOrDefaultAsync(x =>
                        x.ConfiguracionCorreoId == model.ConfiguracionCorreoId &&
                        x.Tipo == model.Tipo &&
                        (tieneUsuario ? x.UsuarioId == model.UsuarioId : x.CorreoManual == model.CorreoManual));

                    if (existente != null)
                    {
                        if (existente.Activo)
                        {
                            return BadRequest("Ese destinatario ya está agregado.");
                        }

                        existente.Activo = true;
                        existente.UsuarioModificoId = usuarioActual?.Id;
                        existente.FechaModificacion = DateTime.Now;
                        await ctx.SaveChangesAsync();
                        return Ok(new { id = existente.Id });
                    }

                    var nuevo = new ConfiguracionCorreoUsuario
                    {
                        ConfiguracionCorreoId = model.ConfiguracionCorreoId,
                        UsuarioId = model.UsuarioId,
                        CorreoManual = model.CorreoManual,
                        Tipo = model.Tipo,
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacionId = usuarioActual?.Id
                    };

                    ctx.ConfiguracionCorreoUsuario.Add(nuevo);
                    await ctx.SaveChangesAsync();
                    return Ok(new { id = nuevo.Id });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/configuracioncorreo/destinatarios/estado/{id}/{usuario}")]
        public async Task<IHttpActionResult> CambiarEstadoDestinatario(int id, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var registro = await ctx.ConfiguracionCorreoUsuario.FindAsync(id);

                    if (registro == null)
                    {
                        return BadRequest("No se encuentra el destinatario.");
                    }

                    var usuarioActual = await ctx.Usuarios.FirstOrDefaultAsync(u => u.usuario == usuario);

                    registro.Activo = !registro.Activo;
                    registro.UsuarioModificoId = usuarioActual?.Id;
                    registro.FechaModificacion = DateTime.Now;

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
