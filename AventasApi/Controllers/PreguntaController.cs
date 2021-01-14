using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class PreguntaController : ApiController
    {

        ///---------------GET Y POST DE ENCUESTAS
        [HttpGet]
        [Route("~/api/preguntas/{seccionId}")]
        public async Task<IHttpActionResult> ObtenerPreguntas(int seccionId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaPreguntas = await ctx.Preguntas.Where(p=> p.SeccionEncuestaId == seccionId).Select(x => new 
                    {
                       Id = x.Id,
                       SeccionEncuestaId = x.SeccionEncuestaId,
                       TipoIngresoId = x.TipoIngresoId,
                       GrupoOpcionesId = x.GrupoOpcionesId,
                       Nombre = x.Nombre,
                       Descripcion = x.Descripcion,
                       Obligatorio = x.Obligatorio,
                       RespuestaObligatorio = x.RespuestaObligatorio,
                       Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaPreguntas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/preguntas/registrar")]
        public async Task<IHttpActionResult> RegistrarPreguntas([FromBody] PreguntasViewModel preguntasEncuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var PreguntasEncuesta = new Preguntas() {
                        SeccionEncuestaId = preguntasEncuesta.SeccionEncuestaId,
                        TipoIngresoId = preguntasEncuesta.TipoIngresoId,
                        GrupoOpcionesId = preguntasEncuesta.GrupoOpcionesId,
                        Nombre = preguntasEncuesta.Nombre,
                        Descripcion = preguntasEncuesta.Descripcion,
                        Obligatorio = preguntasEncuesta.Obligatorio,
                        RespuestaObligatorio = preguntasEncuesta.RespuestaObligatorio,
                        Status = preguntasEncuesta.Status,
                        CreatedBy = preguntasEncuesta.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.Preguntas.Add(PreguntasEncuesta);
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/preguntas/modificar")]
        public async Task<IHttpActionResult> ModificarEncuesta([FromBody] PreguntasViewModel preguntasEncuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var PreguntasBD = await ctx.Preguntas.FindAsync(preguntasEncuesta.Id);

                    if (PreguntasBD == null)
                    {
                        return BadRequest("No se encuentra la pregunta");
                    }
                    PreguntasBD.SeccionEncuestaId = preguntasEncuesta.SeccionEncuestaId;
                    PreguntasBD.TipoIngresoId = preguntasEncuesta.TipoIngresoId;
                    PreguntasBD.GrupoOpcionesId = preguntasEncuesta.GrupoOpcionesId;
                    PreguntasBD.Nombre = preguntasEncuesta.Nombre;
                    PreguntasBD.Descripcion = preguntasEncuesta.Descripcion;
                    PreguntasBD.Obligatorio = preguntasEncuesta.Obligatorio;
                    PreguntasBD.RespuestaObligatorio = preguntasEncuesta.RespuestaObligatorio;
                    PreguntasBD.Status = preguntasEncuesta.Status;
                    PreguntasBD.ModifiedBy = preguntasEncuesta.Usuario;
                    PreguntasBD.ModifiedDate = DateTime.Now;
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