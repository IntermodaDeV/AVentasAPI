using AventasApi.Models;
using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/pantalla")]
    public class PantallaController : ApiController
    {
        [HttpGet]
        [Route("pantallas")]
        public async Task<IHttpActionResult> ObtenerPantallas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var listaPantallas = await ctx.Pantallas.Select(x => new PantallaModel{ Id = x.IdPantalla, Nombre = x.Nombre, Ruta=x.Ruta,Status = x.Status.Value,ModoOffline=x.ModoOffline.Value }).ToListAsync();
                    return Ok(listaPantallas);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("estado/{Id}")]
        public async Task<IHttpActionResult> ModificarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pantalla = await ctx.Pantallas.FindAsync(Id);

                    if (pantalla == null)
                    {
                        return BadRequest("No se encuentra la pantalla.");
                    }

                    pantalla.Status = !pantalla.Status;
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
        [Route("modooffline/{Id}")]
        public async Task<IHttpActionResult> ModificarOffline(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pantalla = await ctx.Pantallas.FindAsync(Id);

                    if (pantalla == null)
                    {
                        return BadRequest("No se encuentra la pantalla.");
                    }

                    pantalla.ModoOffline = !pantalla.ModoOffline;
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
        [Route("crear")]
        public async Task<IHttpActionResult> CrearPantalla([FromBody] PantallaModel pantalla)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var nuevoPantalla = new Pantallas() { Status = pantalla.Status, Nombre = pantalla.Nombre,Ruta=pantalla.Ruta , CreatedBy = pantalla.Usuario, CreatedDate = DateTime.Now,ModoOffline=false};
                    ctx.Pantallas.Add(nuevoPantalla);
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
        [Route("modificar")]
        public async Task<IHttpActionResult> ModificarPantalla([FromBody] PantallaModel pantalla)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pantallaBD = await ctx.Pantallas.FindAsync(pantalla.Id);

                    if (pantallaBD == null)
                    {
                        return BadRequest("No se encuentra la pantalla");
                    }

                    pantallaBD.Nombre = pantalla.Nombre;
                    pantallaBD.Status = pantalla.Status;
                    pantallaBD.Ruta   = pantalla.Ruta;
                    pantallaBD.ModoOffline = pantalla.ModoOffline;
                    pantallaBD.ModifiedBy = pantalla.Usuario;
                    pantallaBD.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/PantallasAsignadas/{Id}")]
        public async Task<IHttpActionResult> GetPantallasAsignadas(int Id)
        {
            try
            {
                using (var pant = new AVentasEntities())
                {
                    var PantallasAsignadas = pant.Pantallas_Funciones.Where(x => x.IdFuncion == Id && x.Status == true).Select(x => x.IdPantalla).ToList();

                    var ListaPantallas = await pant.Pantallas.Where(p => PantallasAsignadas.Contains(p.IdPantalla)).Select(p => new PantallaModel 
                    { 
                        Id = p.IdPantalla
                        ,Nombre = p.Nombre
                        ,Ruta = p.Ruta
                        ,Status = p.Status
                    }).ToListAsync();

                    return Ok(ListaPantallas);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PantallasNoAsignadas/{Id}")]
        public async Task<IHttpActionResult> GetPantallasNoAsignadas(int Id)
        {
            try
            {
                using (var pant = new AVentasEntities())
                {
                    var PantallasAsignadas = pant.Pantallas_Funciones.Where(x => x.IdFuncion == Id && x.Status == true).Select(x => x.IdPantalla).ToList();

                    var ListaPantalla = await pant.Pantallas.Where(p => !PantallasAsignadas.Contains(p.IdPantalla)).Select(p => new PantallaModel
                    {
                         Id = p.IdPantalla
                        ,Nombre = p.Nombre
                        ,Ruta = p.Ruta
                        ,Status = p.Status
                    }).ToListAsync();

                    return Ok(ListaPantalla);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/Api/Pantalla/AsignarPantalla/{IdFuncion}/{IdPantalla}/{usuario}")]
        public async Task<IHttpActionResult> AsignarPantalla(int IdFuncion, int IdPantalla, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var PantallaFuncion = await ctx.Pantallas_Funciones.FirstOrDefaultAsync(x => x.IdFuncion == IdFuncion && x.IdPantalla == IdPantalla);

                    if (PantallaFuncion != null)
                    {
                        PantallaFuncion.Status = true;
                        PantallaFuncion.ModifiedBy = usuario;
                        PantallaFuncion.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var PantallasFuncion = new Pantallas_Funciones() {
                             IdFuncion = IdFuncion
                            ,IdPantalla = IdPantalla
                            ,Status = true
                            ,CreatedBy = usuario
                            ,CreatedDate = DateTime.Now
                            ,ModifiedBy = usuario
                            ,ModifiedDate = DateTime.Now
                        };
                        ctx.Pantallas_Funciones.Add(PantallasFuncion);
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
        [Route("~/api/Pantalla/RemoverPantalla/{IdFuncion}/{IdPantalla}/{usuario}")]
        public async Task<IHttpActionResult> RemoverPantalla(int IdFuncion, int IdPantalla, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var PantallaFuncion = await ctx.Pantallas_Funciones.FirstOrDefaultAsync(x => x.IdPantalla == IdPantalla && x.IdFuncion == IdFuncion);

                    if (PantallaFuncion == null)
                    {
                        return BadRequest("La funcion no tiene asignada esta funcion");
                    }

                    PantallaFuncion.Status = false;
                    PantallaFuncion.ModifiedBy = usuario;
                    PantallaFuncion.ModifiedDate = DateTime.Now;
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
