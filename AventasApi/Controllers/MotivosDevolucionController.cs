using AventasApi.Models;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class MotivosDevolucionController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;
        public MotivosDevolucionController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
            [HttpGet]
        [Route("~/api/motivosDevolucion")]
        public async Task<IHttpActionResult> ObtenerMotivosDevolucion()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaEncuesta = await ctx.MotivosDevolucion.Select(x => new MotivosDevolucionViewModel
                    {
                        IdMotivoDevolucion = x.IdMotivoDevolucion,
                        CodigoMotivoDevolucion = x.CodigoMotivoDevolucion,
                        Descripcion = x.Descripcion,
                        aprobacionObligatoria = x.aprobacionObligatoria,
                        EmpresaId = x.EmpresaId,
                        Estado = x.Estado
                    }).ToListAsync();
                    return Ok(ListaEncuesta);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/ActualizarAprobacionDevolucion/{idMotivoDevolucion}")]
        public async Task<IHttpActionResult> updateAprobacion(int idMotivoDevolucion)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var MotivoDevolucion = ctx.MotivosDevolucion.FirstOrDefault(x => x.IdMotivoDevolucion == idMotivoDevolucion);
                    if(MotivoDevolucion == null)
                    {
                        return BadRequest("No se encuentra el motivo de devolucion");
                    }
                    MotivoDevolucion.aprobacionObligatoria = !MotivoDevolucion.aprobacionObligatoria;
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
        [Route("~/api/motivosDevolucion/usuarios/{Id}")]
        public async Task<IHttpActionResult> GetUsuariosConAcceso(int Id)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var UsuariosConAcceso = db.MotivosDevConAprobacion.Where(x => x.Estado == true && x.IdMotivoDevolucion == Id).Select(x => x.IdUsuario).ToList();
                    var ListaUsuarios = await db.Usuarios.Where(e => UsuariosConAcceso.Contains(e.Id) && e.status == true).Select(e => new UsuarioModel
                    {
                        Id = e.Id,
                        Nombre = e.nombre
                    }).ToListAsync();

                    return Ok(ListaUsuarios);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/motivosDevolucion/sinAccesoUsuarios/{Id}")]
        public async Task<IHttpActionResult> GetUsuariosSinAcceso(int Id)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var UsuariosConAcceso = db.MotivosDevConAprobacion.Where(x => x.Estado == true && x.IdMotivoDevolucion == Id).Select(x => x.IdUsuario).ToList();
                    var ListaUsuarios = await db.Usuarios.Where(e => !UsuariosConAcceso.Contains(e.Id) && e.status == true).Select(e => new UsuarioModel
                    {
                        Id = e.Id,
                        Nombre = e.nombre
                    }).ToListAsync();

                    return Ok(ListaUsuarios);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/motivosDevolucion/AsignarAccesoUsuario/{idMotivoDevolucion}/{usuarioId}")]
        public async Task<IHttpActionResult> AsignarAccesoUsuario(int idMotivoDevolucion, int usuarioId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var MotivosDevConAprov = await db.MotivosDevConAprobacion.FirstOrDefaultAsync(e => e.IdMotivoDevolucion == idMotivoDevolucion && e.IdUsuario == usuarioId);
                    
                    if (MotivosDevConAprov != null)
                    {
                        MotivosDevConAprov.Estado = true;
                        MotivosDevConAprov.UsuarioModifica = user.Id;
                        MotivosDevConAprov.FechaModifica = DateTime.Now;
                    }
                    else
                    {
                        var MotivosDevConApro = new MotivosDevConAprobacion()
                        {
                            IdMotivoDevolucion = idMotivoDevolucion,
                            IdUsuario = usuarioId,
                            Estado = true,
                            UsuarioCrea = user.Id,
                            FechaCrea = DateTime.Now
                        };
                        db.MotivosDevConAprobacion.Add(MotivosDevConApro);
                    }

                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/motivosDevolucion/RemoverUsuario/{idMotivoDevolucion}/{usuarioId}")]
        public async Task<IHttpActionResult> RemoverAccesoUsuario(int idMotivoDevolucion, int usuarioId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var MotivosDevConAprov = await db.MotivosDevConAprobacion.FirstOrDefaultAsync(e => e.IdMotivoDevolucion == idMotivoDevolucion && e.IdUsuario == usuarioId);

                    if (MotivosDevConAprov == null)
                    {
                        return BadRequest("No se encontro el registro, contacte al administrador");
                    }

                    MotivosDevConAprov.Estado = false;
                    MotivosDevConAprov.UsuarioModifica = user.Id;
                    MotivosDevConAprov.FechaModifica = DateTime.Now;
                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
           