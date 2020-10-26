using DBData.Database;
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
        [Route("usuariosactivos")]
        public async Task<IHttpActionResult> ObtenerUsuarioActivos()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var usuariosActivos = await ctx.Usuarios.Where(x=>x.status==true).Select(x => new { Id = x.Id, Usuario = x.usuario }).ToListAsync();
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
    }
}
