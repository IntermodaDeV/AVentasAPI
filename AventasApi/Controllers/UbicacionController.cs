using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/ubicacion")]
    public class UbicacionController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> ObtenerUbicaciones()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var ubicaciones = await ctx.UbicacionesXAlmacen.Include(x=>x.MaestroBodegaAlmacenes).Select(x => new { 
                        UbicacionId=x.UbicacionId,
                        CodigoUbicacion=x.CodigoUbicacion,
                        Almacen=x.MaestroBodegaAlmacenes.Almacen,
                        Etiqueta=x.MaestroBodegaAlmacenes.Etiqueta,
                        Empresa=x.MaestroBodegaAlmacenes.EmpresaId,
                        Estatus=x.Estatus
                    }).ToListAsync();
                    return Ok(ubicaciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
