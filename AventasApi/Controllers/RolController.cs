using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class RolController : ApiController
    {
        [HttpGet]
        [Route("~/api/rol")]
        public async Task<IHttpActionResult> ObtenerRoles()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var listaRoles = await ctx.Roles.Select(x=>new { Id=x.Id,Nombre=x.Nombre,Status=x.Status }).ToListAsync();
                    return Ok(listaRoles);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/rol/rolesactivos")]
        public async Task<IHttpActionResult> ObtenerRolesAcivos()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var listaRoles = await ctx.Roles.Where(x=>x.Status==true).Select(x => new { Id = x.Id, Nombre = x.Nombre, Status = x.Status }).ToListAsync();
                    return Ok(listaRoles);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/rol/funciones/{id}")]
        public async Task<IHttpActionResult> ObtenerFuncionesRoles(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var listaFuncionesAsignadas = await ctx.Funciones_Roles
                        .Include(x=>x.Funciones)
                        .Where(x => x.IdRol == id && x.Status==true)
                        .Select(x=>x.Funciones)
                        .ToListAsync();

                    var listaFuncionModel =  listaFuncionesAsignadas.Select(x => new FuncionesViewModel {Id=x.Id,Status=x.Status,Nombre=x.Nombre }).ToList();

                    return Ok(listaFuncionModel);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/rol/funcionesnoasignadas/{id}")]
        public async Task<IHttpActionResult> ObtenerFuncionesRolesNoAsgnadas(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var listaFuncionesAsignadas = ctx.Funciones_Roles
                        .Include(x => x.Funciones)
                        .Where(x => x.IdRol == id && x.Status==true)
                        .Select(x => x.Funciones);

                    var funcionesNoAsignadas =await  ctx.Funciones
                        .Except(listaFuncionesAsignadas)
                        .Select(x => new FuncionesViewModel { Id = x.Id, Status = x.Status, Nombre = x.Nombre })
                        .ToListAsync();

                    return Ok(funcionesNoAsignadas);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("~/api/rol/asignarfuncion/{idrol}/{idfuncion}/{usuario}")]
        public async Task<IHttpActionResult> AsignarFuncionRol(int idrol,int idfuncion,string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var funcionRol = await ctx.Funciones_Roles.FirstOrDefaultAsync(x => x.IdFuncion == idfuncion && x.IdRol == idrol);

                    if (funcionRol != null)
                    {
                        funcionRol.Status = true;
                        funcionRol.modifiedby = usuario;
                        funcionRol.modifieddate = DateTime.Now;
                    }
                    else
                    {
                        var rolFuncion = new Funciones_Roles() { IdFuncion = idfuncion, IdRol = idrol, Status = true,createdby=usuario,createddate=DateTime.Now,modifiedby=usuario,modifieddate=DateTime.Now };
                        ctx.Funciones_Roles.Add(rolFuncion);
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
        [Route("~/api/rol/removerfuncion/{idrol}/{idfuncion}/{usuario}")]
        public async Task<IHttpActionResult> RemoverFuncionRol(int idrol, int idfuncion,string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var funcionRol = await ctx.Funciones_Roles.FirstOrDefaultAsync(x => x.IdFuncion == idfuncion && x.IdRol == idrol);

                    if (funcionRol == null)
                    {
                        return BadRequest("El rol no tiene asignada la funcion.");
                    }

                    funcionRol.Status = false;
                    funcionRol.modifiedby = usuario;
                    funcionRol.modifieddate = DateTime.Now;
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
        [Route("~/api/rol/estado/{Id}")]
        public async Task<IHttpActionResult> ModificarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rol = await ctx.Roles.FindAsync(Id);

                    if (rol == null)
                    {
                        return BadRequest("No se encuentra el rol.");
                    }

                    rol.Status = !rol.Status;
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
        [Route("~/api/rol/crear")]
        public async Task<IHttpActionResult> CrearRol([FromBody] RolCrearModel rol)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var nuevoRol = new Roles() { Status = rol.Status, Nombre = rol.Nombre };
                    ctx.Roles.Add(nuevoRol);
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
        [Route("~/api/rol/modificar")]
        public async Task<IHttpActionResult> ModificarRol([FromBody] RolCrearModel rol)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var rolBd = await ctx.Roles.FindAsync(rol.Id);

                    if(rolBd == null)
                    {
                        return BadRequest("No se encuentra el rol");
                    }

                    rolBd.Nombre = rol.Nombre;
                    rolBd.Status = rol.Status;
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
