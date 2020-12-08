using AventasApi.Models;
using DBData.Database;
using ExternalApiData.ApiModels;
using ExternalApiData.Enviroments;
using RestSharp;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/usuario")]
    public class UsuarioController : ApiController
    {

        [HttpGet]
        public async Task<IHttpActionResult> ObtenerUsuarios()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuarios = await ctx.Usuarios.Select(x => new {
                        Id = x.Id, 
                        Usuario = x.usuario,
                        Nombre=x.nombre,
                        Status=x.status,
                        BloqueoCredito=x.BloqueoInfoCredito,
                        BloqueoAsesores=x.FlagTodosAsesores,
                        UsuarioOficina = x.FlagUsuarioOficina
                    }).ToListAsync();
                    return Ok(usuarios);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> CrearUsuarios([FromBody] UsuarioModel usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuarioBd = await ctx.Usuarios.FirstOrDefaultAsync(x => x.usuario == usuario.usuario);

                    if (usuarioBd != null)
                    {
                        return BadRequest("El usuario ya ha sido registrado anteriormente.");
                    }

                    var newUsuario = new Usuarios()
                    {
                        usuario = usuario.usuario,
                        nombre = usuario.nombre,
                        EmpresaId = usuario.EmpresaId,
                        BloqueoInfoCredito=false,
                        FlagTodosAsesores=false,
                        FlagUsuarioOficina = false,
                        CreatedBy = usuario.creador,
                        CreatedDate=DateTime.Now,
                        ModifiedBy=usuario.creador,
                        ModifiedDate=DateTime.Now,
                        status=true
                    };

                    ctx.Usuarios.Add(newUsuario);
                    var result = await ctx.SaveChangesAsync();
                    return Ok(newUsuario);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("verificar/{usuario}")]
        public IHttpActionResult VerificarUsuario(string usuario)
        {
            try
            {
                var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}usuario/{usuario}");
                client.Timeout = 480 * (1000);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response =  client.Execute(request);

                if (!response.IsSuccessful)
                {
                    return BadRequest("Servidor se encuentra fuera de linea.");
                }

                var content = Newtonsoft.Json.JsonConvert.DeserializeObject<UsuarioApiModel>(response.Content);
                return Ok(content);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("usuariosactivos")]
        public async Task<IHttpActionResult> ObtenerUsuarioActivos()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var usuariosActivos = await ctx.Usuarios.Where(x=>x.status==true).Select(x => new { Id = x.Id, Usuario = x.nombre }).ToListAsync();
                    return Ok(usuariosActivos);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("roles/{id}")]
        public async Task<IHttpActionResult> ObtenerRolesUsuario(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolAsignados = await ctx.Usuario_Rol
                        .Include(x => x.Roles)
                        .Where(x => x.usuarioId == id && x.status == true)
                        .Select(x => x.Roles)
                        .ToListAsync();

                    var listaRoles = rolAsignados.Select(x => new { Id = x.Id, Nombre = x.Nombre, Status = x.Status }).ToList();
                    return Ok(listaRoles);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("rolesnoasignados/{id}")]
        public async Task<IHttpActionResult> ObtenerRolesUsuarioInactivos(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolAsignados =  ctx.Usuario_Rol
                        .Include(x => x.Roles)
                        .Where(x => x.usuarioId == id && x.status == true)
                        .Select(x => x.Roles);

                    var rolNoAsignados = await ctx.Roles
                        .Except(rolAsignados)
                        .Select(x => new  { Id = x.Id, Status = x.Status, Nombre = x.Nombre })
                        .ToListAsync();

                    return Ok(rolNoAsignados);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("asignarrol/{usuarioId}/{rolId}/{usuario}")]
        public async Task<IHttpActionResult> AsignarUsuarioRol(int usuarioId, int rolId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuarioRol = await ctx.Usuario_Rol.FirstOrDefaultAsync(x => x.usuarioId == usuarioId && x.rolId == rolId);

                    if (usuarioRol != null)
                    {
                        usuarioRol.status = true;
                        usuarioRol.editedBy = usuario;
                        usuarioRol.editedDate = DateTime.Now;
                    }
                    else
                    {
                        var newUsuarioRol = new Usuario_Rol() { usuarioId = usuarioId, rolId = rolId, status = true, createdBy = usuario, createdDate = DateTime.Now, editedBy = usuario, editedDate = DateTime.Now };
                        ctx.Usuario_Rol.Add(newUsuarioRol);
                    }

                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("removerrol/{usuarioId}/{rolId}/{usuario}")]
        public async Task<IHttpActionResult> RemoverUsuarioRol(int usuarioId, int rolId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuarioRol = await ctx.Usuario_Rol.FirstOrDefaultAsync(x => x.usuarioId == usuarioId && x.rolId == rolId);

                    if (usuarioRol == null)
                    {
                        return BadRequest("El usuario no tiene asignado el rol.");
                    }

                    usuarioRol.status = false;
                    usuarioRol.editedBy = usuario;
                    usuarioRol.editedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("desactivar/{id}/{user}")]
        public async Task<IHttpActionResult> RemoverUsuarioRol(int id,string user)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = await ctx.Usuarios.FindAsync(id);

                    if (usuario == null)
                    {
                        return BadRequest("El usuario no existe.");
                    }

                    usuario.status = !usuario.status;
                    usuario.ModifiedDate = DateTime.Now;
                    usuario.ModifiedBy = user;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("desactivar/sensible/{id}/{user}")]
        public async Task<IHttpActionResult> RemoverUsuarioBloqueSensible(int id, string user)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = await ctx.Usuarios.FindAsync(id);

                    if (usuario == null)
                    {
                        return BadRequest("El usuario no existe.");
                    }

                    usuario.BloqueoInfoCredito = !usuario.BloqueoInfoCredito;
                    usuario.ModifiedDate = DateTime.Now;
                    usuario.ModifiedBy = user;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("desactivar/asesores/{id}/{user}")]
        public async Task<IHttpActionResult> RemoverTodosAsesores(int id, string user)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = await ctx.Usuarios.FindAsync(id);

                    if (usuario == null)
                    {
                        return BadRequest("El usuario no existe.");
                    }

                    usuario.FlagTodosAsesores = !usuario.FlagTodosAsesores;
                    usuario.ModifiedDate = DateTime.Now;
                    usuario.ModifiedBy = user;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("usuarioOficina/{id}/{estado}/{user}")]
        public async Task<IHttpActionResult> UpdateUsuarioOficina(int id, bool estado, string user)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = await ctx.Usuarios.FindAsync(id);

                    if (usuario == null)
                    {
                        return BadRequest("El usuario no existe.");
                    }

                    usuario.FlagUsuarioOficina = estado;
                    usuario.ModifiedDate = DateTime.Now;
                    usuario.ModifiedBy = user;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
