using AventasApi.Models;
using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

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
                    var listaPantallas = await ctx.Pantallas.Select(x => new PantallaModel{ Id = x.IdPantalla, Nombre = x.Nombre, Ruta=x.Ruta,Status = x.Status.Value }).ToListAsync();
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
        [Route("crear")]
        public async Task<IHttpActionResult> CrearPantalla([FromBody] PantallaModel pantalla)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var nuevoPantalla = new Pantallas() { Status = pantalla.Status, Nombre = pantalla.Nombre,Ruta=pantalla.Ruta };
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
                        return BadRequest("No se encuentra el rol");
                    }

                    pantallaBD.Nombre = pantalla.Nombre;
                    pantallaBD.Status = pantalla.Status;
                    pantallaBD.Ruta   = pantalla.Ruta;
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
