using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class EncuestaController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public EncuestaController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        ///--------------- ENCUESTAS
        [HttpGet]
        [Route("~/api/Encuesta")]
        public async Task<IHttpActionResult> ObtenerEncuestas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaEncuesta = await ctx.Encuesta.Select(x => new 
                    {
                        Id =x.Id,
                        Nombre = x.Nombre,
                        Descripcion = x.Descripcion,
                        FechaInicio = x.FechaInicio,
                        FechaFin = x.FechaFin
                    }).ToListAsync();
                    return Ok(ListaEncuesta);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Encuesta/registrar")]
        public async Task<IHttpActionResult> RegistrarEncuesta([FromBody] EncuestaViewModel encuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Encuesta = new Encuesta() {
                        Nombre = encuesta.Nombre,
                        Descripcion = encuesta.Descripcion,
                        FechaInicio = encuesta.FechaInicio,
                        FechaFin = encuesta.FechaFin,
                        CreatedBy = encuesta.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.Encuesta.Add(Encuesta);
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
        [Route("~/api/Encuesta/modificar")]
        public async Task<IHttpActionResult> ModificarEncuesta([FromBody] EncuestaViewModel encuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaBD = await ctx.Encuesta.FindAsync(encuesta.Id);

                    if (EncuestaBD == null)
                    {
                        return BadRequest("No se encuentra el tipo ingreso");
                    }

                    EncuestaBD.Nombre = encuesta.Nombre;
                    EncuestaBD.Descripcion = encuesta.Descripcion;
                    EncuestaBD.FechaInicio = encuesta.FechaInicio;
                    EncuestaBD.FechaFin = encuesta.FechaFin;
                    EncuestaBD.ModifiedBy = encuesta.Usuario;
                    EncuestaBD.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }


        ///---------------------------EMPRESAS ENCUESTAS ---------------------------------------------------------
        [HttpGet]
        [Route("~/api/Encuesta/EmpresaPermitidas/{Id}")]
        public async Task<IHttpActionResult> GetEmpresaPermitidas(int Id)
        {
            try
            {
                using (var emp = new AVentasEntities())
                {
                    var EmpresasPermitidas = await emp.Empresa_Encuesta.Where(x => x.EncuestaId == Id && x.Status == true).Select(x => x.EmpresaId).ToListAsync();

                    var ListaEmpresas = await emp.Empresa.Where(e => EmpresasPermitidas.Contains(e.EmpresaId)).Select(e => new EmpresaViewModel
                    {
                        Id = e.EmpresaId,
                        Nombre = e.EmpresaId
                    }).ToListAsync();

                    return Ok(ListaEmpresas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Encuesta/EmpresasNoPermitidas/{Id}")]
        public async Task<IHttpActionResult> GetEmpresasNoPermitidas(int Id)
        {
            try
            {
                using (var emp = new AVentasEntities())
                {
                    var EmpresasPermitidas = await emp.Empresa_Encuesta.Where(x => x.EncuestaId == Id && x.Status == true).Select(x => x.EmpresaId).ToListAsync();

                    var ListaEmpresas = await emp.Empresa.Where(e => !EmpresasPermitidas.Contains(e.EmpresaId)).Select(e => new EmpresaViewModel
                    {
                        Id = e.EmpresaId,
                        Nombre = e.EmpresaId
                    }).ToListAsync();

                    return Ok(ListaEmpresas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("~/api/Encuesta/AsignarEmpresa/{EmpresaId}/{encuestaId}/{usuario}")]
        public async Task<IHttpActionResult> AsignarEmpresa(string EmpresaId, int encuestaId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaEmpresa = await ctx.Empresa_Encuesta.FirstOrDefaultAsync(x => x.EncuestaId == encuestaId && x.EmpresaId == EmpresaId);

                    if (EncuestaEmpresa != null)
                    {
                        EncuestaEmpresa.Status = true;
                        EncuestaEmpresa.ModifiedBy = usuario;
                        EncuestaEmpresa.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var Empresa_Encuesta = new Empresa_Encuesta()
                        {
                            EmpresaId = EmpresaId,
                            EncuestaId = encuestaId,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.Empresa_Encuesta.Add(Empresa_Encuesta);
                    }

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
        [Route("~/api/Encuesta/RemoverEmpresa/{EmpresaId}/{EncuestaId}/{usuario}")]
        public async Task<IHttpActionResult> RemoverEmpresa(string EmpresaId, int EncuestaId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EmpresaEncuesta = await ctx.Empresa_Encuesta.FirstOrDefaultAsync(x => x.EmpresaId == EmpresaId && x.EncuestaId == EncuestaId);

                    if (EmpresaEncuesta == null)
                    {
                        return BadRequest("No se encontro el registro, contacte al administrador");
                    }

                    EmpresaEncuesta.Status = false;
                    EmpresaEncuesta.ModifiedBy = usuario;
                    EmpresaEncuesta.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
        ///---------------SECCIONES ENCUESTAS -----------------------------------------------------------------------------
        [HttpGet]
        [Route("~/api/Encuesta/SeccionesPermitidas/{Id}")]
        public async Task<IHttpActionResult> GetSeccionesEncuestaPermitidas(int Id)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var SeccionesPermitidas = await db.Secciones_Encuesta.Where(x => x.EncuestaId == Id && x.Status == true).Select(x => x.SeccionId).ToListAsync();

                    var ListaSecciones = await db.Secciones.Where(e => SeccionesPermitidas.Contains(e.Id)).Select(e => new SeccionEncuestaViewModel
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Titulo = e.Titulo
                    }).ToListAsync();

                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Encuesta/SeccionesNoPermitidas/{Id}")]
        public async Task<IHttpActionResult> GetSeccionesEncuestaNoPermitidas(int Id)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var SeccionesNoPermitidas = await db.Secciones_Encuesta.Where(x => x.EncuestaId == Id && x.Status == true).Select(x => x.SeccionId).ToListAsync();

                    var ListaSecciones = await db.Secciones.Where(e => !SeccionesNoPermitidas.Contains(e.Id) && e.Status == true).Select(e => new SeccionEncuestaViewModel
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Titulo = e.Titulo
                    }).ToListAsync();

                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Encuesta/AsignarSecciones/{EncuestaId}/{seccionId}/{usuario}")]
        public async Task<IHttpActionResult> AsignarEncuestaSeccion(int EncuestaId, int seccionId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var SeccionesEncuesta = await ctx.Secciones_Encuesta.FirstOrDefaultAsync(x => x.EncuestaId == EncuestaId && x.SeccionId == seccionId);

                    if (SeccionesEncuesta != null)
                    {
                        SeccionesEncuesta.Status = true;
                        SeccionesEncuesta.ModifiedBy = usuario;
                        SeccionesEncuesta.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var Secciones_Encuesta = new Secciones_Encuesta()
                        {
                            SeccionId = seccionId,
                            EncuestaId = EncuestaId,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.Secciones_Encuesta.Add(Secciones_Encuesta);
                    }

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
        [Route("~/api/Encuesta/RemoverSeccion/{seccionId}/{EncuestaId}/{usuario}")]
        public async Task<IHttpActionResult> RemoverSeccionesEncuesta(int seccionId, int EncuestaId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var SeccionEncuesta = await ctx.Secciones_Encuesta.FirstOrDefaultAsync(x => x.SeccionId == seccionId && x.EncuestaId == EncuestaId);

                    if (SeccionEncuesta == null)
                    {
                        return BadRequest("No se encontro el registro, contacte al administrador");
                    }

                    SeccionEncuesta.Status = false;
                    SeccionEncuesta.ModifiedBy = usuario;
                    SeccionEncuesta.ModifiedDate = DateTime.Now;
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
        [Route("~/api/Encuesta/Seccion/{encuestaId}")]
        public async Task<IHttpActionResult> ObtenerSeccionEncuesta(int encuestaId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var SeccionesList = await ctx.Secciones_Encuesta.Where(e => e.EncuestaId == encuestaId && e.Status == true).Select(x => x.SeccionId).ToListAsync();
                    var ListaSecciones = await ctx.Secciones.Where(e=> SeccionesList.Contains(e.Id)).Select(x => new
                    {
                        Id = x.Id,
                        EncuestaId = encuestaId,
                        NombreEncuesta = "",
                        Nombre = x.Nombre,
                        Titulo = x.Titulo,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("~/api/Encuesta/Seccion")]
        public async Task<IHttpActionResult> ObtenerSeccion()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaSecciones = await ctx.Secciones.Select(x => new
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Titulo = x.Titulo,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("~/api/Encuesta/Secciones/registrar")]
        public async Task<IHttpActionResult> RegistrarSecciones([FromBody] SeccionEncuestaViewModel seccion)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var seccionEncuesta = new Secciones()
                    {
                        Nombre = seccion.Nombre,
                        Descripcion = seccion.Descripcion,
                        Titulo = seccion.Titulo,
                        Obligatorio = seccion.Obligatorio,
                        Status = seccion.Status,
                        CreatedBy = seccion.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.Secciones.Add(seccionEncuesta);
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
        [Route("~/api/secciones/modificar")]
        public async Task<IHttpActionResult> ModificarSeccion([FromBody] SeccionEncuestaViewModel secciones)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaBD = await ctx.Secciones.FindAsync(secciones.Id);

                    if (EncuestaBD == null)
                    {
                        return BadRequest("No se encuentra la seccione de encuesta");
                    }
                    EncuestaBD.Nombre = secciones.Nombre;
                    EncuestaBD.Descripcion = secciones.Descripcion;
                    EncuestaBD.Titulo = secciones.Titulo;
                    EncuestaBD.Obligatorio = secciones.Obligatorio;
                    EncuestaBD.Status = secciones.Status;
                    EncuestaBD.ModifiedBy = secciones.Usuario;
                    EncuestaBD.ModifiedDate = DateTime.Now;
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
        [Route("~/api/secciones/estado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var seccion = await ctx.Secciones.FindAsync(Id);

                    if (seccion == null)
                    {
                        return BadRequest("No se encuentra la sección de encuesta.");
                    }

                    seccion.Status = !seccion.Status;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        ///---------------SECCIONES USUARIOS --------------------------------------------------------------------------------

        [HttpGet]
        [Route("~/api/secciones/usuarios/{Id}/{encuestaId}")]
        public async Task<IHttpActionResult> GetUsuariosConAcceso(int Id, int encuestaId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var encuesta = await db.Secciones_Encuesta.FirstOrDefaultAsync(e => e.SeccionId == Id && e.EncuestaId == encuestaId);
                    var UsuariosConAcceso = db.Secciones_Usuarios.Where(x => x.SeccionId == encuesta.Id && x.Status == true).Select(x => x.UsuarioId).ToList();
                    var ListaUsuarios = await db.Usuarios.Where(e => UsuariosConAcceso.Contains(e.Id)).Select(e => new UsuarioModel
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
        [Route("~/api/secciones/usuariosSinAcceso/{Id}/{encuestaId}")]
        public async Task<IHttpActionResult> GetUsuariosSinAcceso(int Id , int encuestaId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var encuesta = await db.Secciones_Encuesta.FirstOrDefaultAsync(e => e.SeccionId == Id && e.EncuestaId == encuestaId);
                    var UsuariosConAcceso = await db.Secciones_Usuarios.Where(x => x.SeccionId == encuesta.Id && x.Status == true).Select(x => x.UsuarioId).ToListAsync();
                    var EmpresaEncuesta = await db.Empresa_Encuesta.Where(x => x.EncuestaId == encuestaId && x.Status == true).Select(x=> x.EmpresaId).ToListAsync();
                    var ListaUsuarios = await db.Usuarios.Where(e => !UsuariosConAcceso.Contains(e.Id) && EmpresaEncuesta.Contains(e.EmpresaId)).Select(e => new UsuarioModel
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
        [Route("~/api/secciones/AsignarAccesoUsuario/{seccionId}/{usuarioId}/{usuario}/{encuestaId}")]
        public async Task<IHttpActionResult> AsignarAccesoUsuario(int seccionId, int usuarioId, string usuario, int encuestaId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var encuesta = await db.Secciones_Encuesta.FirstOrDefaultAsync(e => e.SeccionId == seccionId && e.EncuestaId == encuestaId);
                    var ListaUsuarios = await db.Secciones_Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId && u.SeccionId == encuesta.Id);

                    if (ListaUsuarios != null)
                    {
                        ListaUsuarios.Status = true;
                        ListaUsuarios.ModifiedBy = usuario;
                        ListaUsuarios.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var Secciones_Usuarios = new Secciones_Usuarios()
                        {
                            SeccionId  = encuesta.Id,
                            UsuarioId = usuarioId,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        db.Secciones_Usuarios.Add(Secciones_Usuarios);
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
        [Route("~/api/secciones/RemoverUsuario/{usuarioId}/{seccionId}/{usuario}/{encuestaId}")]
        public async Task<IHttpActionResult> RemoverAccesoUsuario(int usuarioId, int seccionId, string usuario, int encuestaId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var encuesta = await ctx.Secciones_Encuesta.FirstOrDefaultAsync(e => e.SeccionId == seccionId && e.EncuestaId == encuestaId);
                    var UsuarioSeccion = await ctx.Secciones_Usuarios.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.SeccionId == encuesta.Id);

                    if (UsuarioSeccion == null)
                    {
                        return BadRequest("No se encontro el registro, contacte al administrador");
                    }

                    UsuarioSeccion.Status = false;
                    UsuarioSeccion.ModifiedBy = usuario;
                    UsuarioSeccion.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        ///----------------------------RESPUESTA DE ENCUESTAS -------------------------------------------------------------///
        [HttpGet]
        [Route("~/api/Encuesta/{empresa}")]
        public async Task<IHttpActionResult> ObtenerEncuestasActivas(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaId = ctx.Empresa_Encuesta.Where(e => e.EmpresaId == empresa && e.Status == true).Select(e => e.EncuestaId).ToList();
                    var ListaEncuesta = await ctx.Encuesta.Where(e => EncuestaId.Contains(e.Id) && e.FechaInicio <= DateTime.Today && e.FechaFin >= DateTime.Today).Select(x => new
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Descripcion = x.Descripcion,
                        FechaInicio = x.FechaInicio,
                        FechaFin = x.FechaFin
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
        [Route("~/api/EncuestaSelected/{encuestaId}/{usuario}")]
        public async Task<IHttpActionResult> SeccionesEncuestasSelected(int encuestaId, string usuario)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var SeccionesPermitidas = await db.Secciones_Usuarios.Where(p => p.Usuarios.usuario == usuario && p.Status == true).Select(s => s.SeccionId).ToListAsync();
                    var ListaSeccion = await db.Secciones_Encuesta.Where(p => SeccionesPermitidas.Contains(p.Id) && p.EncuestaId == encuestaId && p.Status == true).Select(s => s.SeccionId).ToListAsync();
                    var ListaSecciones = await db.Secciones.Where(e => e.Status == true && ListaSeccion.Contains(e.Id)).Select(x => new
                    {
                        Id = x.Id,
                        EncuestaId = encuestaId,
                        NombreEncuesta = db.Encuesta.FirstOrDefault(e => e.Id == encuestaId).Nombre,
                        Nombre = x.Nombre,
                        Titulo = x.Titulo,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status,
                        Preguntas = x.Preguntas.Where(p => p.SeccionEncuestaId == x.Id && p.Status == true).Select(p => new
                        {
                            PreguntaId = p.Id,
                            TipoIngresoId = p.TipoIngresoId,
                            TipoIngreso = p.TiposIngreso.Nombre,
                            GrupoOpcionesId = p.GrupoOpcionesId,
                            GrupoOpciones = p.GrupoOpciones.Nombre,
                            PreguntasOpciones = p.PreguntasOpciones.Where(o => o.Status == true).Select(po => new
                            {
                                PreguntasOpcionesId = po.Id,
                                GrupoOpcionesDetalleId = po.GrupoOpcionesDetalleId,
                                GOpcionesDetalleNombre = po.GrupoOpcionesDetalle.Nombre
                            }),
                            Nombre = p.Nombre,
                            Descripcion = p.Descripcion,
                            Obligatorio = p.Obligatorio,
                            RespuestaObligatorio = p.RespuestaObligatorio,
                            Status = p.Status
                        }).OrderBy(p => p.PreguntaId)
                    }).ToListAsync();
                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/encuesta/resueltas/{inicio}/{final}/{asesor}")]
        public async Task<IHttpActionResult> EncuestasResueltas(DateTime inicio,DateTime final,string asesor)
       {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    List<EncuestaCompletada> encuestasResueltas = await (from e in ctx.Encuesta
                                              join r in ctx.Respuestas on e.Id equals r.EncuestaId
                                              where r.CreatedDate >= inicio && r.CreatedDate <= final && r.CreatedBy==asesor
                                              select new EncuestaCompletada()
                                              {
                                                  RespuestaId = r.Id,
                                                  EncuestaId =e.Id,
                                                  Encuesta = e.Nombre,
                                                  Cliente = r.CodigoCliente,
                                                  Usuario = r.CreatedBy,
                                                  Fecha = r.CreatedDate
                                              }).ToListAsync();

                    return Ok(encuestasResueltas);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/encuesta/resueltas/{inicio}/{final}/todos")]
        public async Task<IHttpActionResult> EncuestasResueltasTodos(DateTime inicio, DateTime final)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<EncuestaCompletada> encuestasCompletadas = new List<EncuestaCompletada>();

                    foreach(var asesor in asesoresHabilitados) {
                        List<EncuestaCompletada> encuestasResueltas = await (from e in ctx.Encuesta
                                                                             join r in ctx.Respuestas on e.Id equals r.EncuestaId
                                                                             where r.CreatedDate >= inicio && r.CreatedDate <= final && r.CreatedBy == asesor
                                                                             select new EncuestaCompletada()
                                                                             {
                                                                                 EncuestaId = e.Id,
                                                                                 Encuesta = e.Nombre,
                                                                                 Cliente = r.CodigoCliente,
                                                                                 Usuario = r.CreatedBy,
                                                                                 Fecha = r.CreatedDate
                                                                             }).ToListAsync();
                        encuestasCompletadas.AddRange(encuestasResueltas);
                    }

                    return Ok(encuestasCompletadas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/encuesta/resueltas/detalle/{respuestaId}")]
        public async Task<IHttpActionResult> EncuestaResueltaDetalle(int respuestaId)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    List<RespuestasViewModel> Respuestas = await ctx.Respuestas.Where(r => r.Id == respuestaId).Select(r => new RespuestasViewModel
                    {
                        EncuestaId = r.EncuestaId,
                        CodigoCliente = r.CodigoCliente,
                        UsuarioId = r.UsuarioId,
                        RespuestasDetalle = r.RespuestaDetalle.Where(d=> d.RespuestaId == r.Id).Select(d => new RespuestasDetalleViewModel
                        {
                            PreguntaId = d.PreguntaId,
                            PreguntasOpcionesId = d.PreguntaOpcionesId,
                            RespuestaAlfanumerica = d.RespuestaAlfanumerica,
                            RespuestaNumerica = d.RespuestaNumerica
                        }).ToList()
                    }).ToListAsync();
                    return Ok(Respuestas);
                }
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}