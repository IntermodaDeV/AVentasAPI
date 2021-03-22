using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/bodega")]
    public class BodegaController : ApiController
    {
        [HttpGet]
        [Route("almacen")]
        public async Task<IHttpActionResult> ObtenerAlmacenes()
        {
            try
            {
                using(AVentasEntities ctx=new AVentasEntities())
                {
                    var bodegas = await ctx.MaestroBodegaAlmacenes.Where(x=>x.Estatus==true).Select(x => new {Id=x.AlmacenId,Nombre=x.Nombre,Almacen=x.Almacen,SitioId=x.SitioId,Empresa=x.EmpresaId}).ToListAsync();
                    return Ok(bodegas);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("sitio")]
        public async Task<IHttpActionResult> ObtenerSitios()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var bodegas = await ctx.MaestroBodegaSitios.Where(x=>x.Estatus==true).Select(x => new { Id = x.SitioId, Sitio = x.Sitio, Nombre = x.Nombre, Empresa = x.EmpresaId }).ToListAsync();
                    return Ok(bodegas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
