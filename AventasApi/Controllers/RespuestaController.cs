using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class RespuestaController : ApiController
    {

        ///---------------RESPUESTAS
        [HttpGet]
        [Route("~/api/preguntas")]
        public async Task<IHttpActionResult> ObtenerPreguntas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaRespuestas = await ctx.Preguntas.Select(x => new 
                    {
                        Id =x.Id,
                        SeccionEncuestaId = x.SeccionEncuestaId,
                        TipoIngresoId = x.TipoIngresoId,
                        GrupoOpcionesId = x.GrupoOpcionesId,
                        Nombre = x.Nombre,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaRespuestas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}