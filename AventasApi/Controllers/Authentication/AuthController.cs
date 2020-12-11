using AventasApi.Models.Authentication;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using DBData.Database;
using System;
//using Responses;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers.Authentication
{
    [RoutePrefix("api")]
    public class AuthController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;
        public AuthController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        [HttpPost, Route("authentication")]
        public IHttpActionResult Authentication([FromBody] Credential credential)
        {
            var answer = _authenticationAppService.Authentication(credential);

            if (answer.Type != "1")
            {
                return BadRequest(answer.Message);
            }

            return Ok(answer);
        }


        [HttpGet]
        [Route("Accesos/{Usuario}")]
        public async Task<IHttpActionResult> GetPermisos(string Usuario)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    List<string> AsesoresUsuario = new List<string>();

                    var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.usuario == Usuario);
                    var empresas = await db.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == usuario.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        AsesoresUsuario = await db.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await db.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == usuario.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        AsesoresUsuario = await db.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<PermisosViewModel> PermisosUsuario = db.Usuarios.Where(u => u.usuario == Usuario).Select(u => new PermisosViewModel
                    {
                        usuario = u.usuario,
                        EmpresaId = u.EmpresaId,
                        BloqueoCredito=u.BloqueoInfoCredito,
                        status = u.status,
                        TodosAsesores = u.FlagTodosAsesores,
                        UsuarioOficina = u.FlagUsuarioOficina,
                        EmpresasUsuarios = db.Usuarios_Empresas.Where(e => e.Status == true && e.UsuarioId == u.Id).Select(e => new UsuariosEmpresasViewModel
                        {
                            EmpresaId = e.EmpresaId
                        }).ToList(),
                        RolesUsuarios = db.Usuario_Rol.Where(r => r.usuarioId == u.Id && r.status == true).Select(rol => new RolesUsuariosViewModel
                        {
                            Id = rol.Id,
                            Nombre = rol.Roles.Nombre,
                            Usuario = rol.Usuarios.usuario,
                            RolesFunciones = db.Funciones_Roles.Where(f=> f.IdRol == rol.rolId && f.Status == true).Select(f => new RolesFuncionesViewModel
                            {
                                Id = f.Id,
                                Funcion = f.Funciones.Nombre,
                                Status = f.Status,
                                PantallasFunciones = db.Pantallas_Funciones.Where(p => p.IdFuncion == f.IdFuncion && p.Status == true).Select(p => new PantallasFuncionesViewModel
                                {
                                   NombrePantalla = p.Pantallas.Nombre,
                                   Ruta = p.Pantallas.Ruta,
                                   ModoOffline = p.Pantallas.ModoOffline,
                                   Status = p.Status
                                }).ToList()
                            }).ToList()
                        }).ToList(),
                        AsesoresUsuario = db.Asesores.Where(a => AsesoresUsuario.Contains(a.CodigoAsesor)).Select(a => new AsesoresUsuarioViewModel
                        { 
                            Usuario = a.CodigoAsesor

                        }).ToList(),
                    }).ToList();

                    return Ok(PermisosUsuario);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
