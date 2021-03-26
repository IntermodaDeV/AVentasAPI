using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/sitios")]
    public class SitiosController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> ObtenerSitios()
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var sitios = await ctx.MaestroBodegaSitios.Select(x => new {SitioId=x.SitioId,Sitio=x.Sitio,Nombre=x.Nombre,Empresa=x.EmpresaId,Estatus=x.Estatus }).ToListAsync();
                    return Ok(sitios);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("modificar/{usuario}/{id}")]
        public async Task<IHttpActionResult> ModificarSitio(string usuario,int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var sitio = await ctx.MaestroBodegaSitios.FindAsync(id);

                    if (sitio == null)
                    {
                        return BadRequest("El sitio no existe");
                    }

                    sitio.Estatus = !sitio.Estatus;
                    sitio.ModificadoPor = usuario;
                    sitio.FechaModificacion = DateTime.Now;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
