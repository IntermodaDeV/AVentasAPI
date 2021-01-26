using System;
using System.Collections.Generic;
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

        ///---------------RESPUESTAS ----------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("~/api/respuestas/registrar")]
        public async Task<IHttpActionResult> RegistrarRespuesta([FromBody] RespuestasViewModel Respuestas)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    List<RespuestasViewModel> RespuestasEncuestas = new List<RespuestasViewModel>();
                    RespuestasEncuestas.Add(Respuestas);

                    var RespuestaDB = await db.Respuestas.FirstOrDefaultAsync(r => r.CodigoCliente == Respuestas.CodigoCliente && r.EncuestaId == Respuestas.EncuestaId && r.UsuarioId == Respuestas.UsuarioId);
                    var respuestaId = 0;
                    if(RespuestaDB == null)
                    {
                        var Respuesta = new Respuestas()
                        {
                            CodigoCliente = Respuestas.CodigoCliente,
                            UsuarioId = Respuestas.UsuarioId,
                            EncuestaId = Respuestas.EncuestaId,
                            CreatedBy = Respuestas.Usuario,
                            CreatedDate = DateTime.Now
                        };

                        db.Respuestas.Add(Respuesta);
                        var result = await db.SaveChangesAsync();

                        respuestaId = db.Respuestas.OrderByDescending(r => r.Id).Select(r => r.Id).FirstOrDefault();
                    }
                    else
                    {
                        respuestaId = RespuestaDB.Id;
                    }

                    _ = RegistrarRespuestasDetalle(Respuestas.RespuestasDetalle, respuestaId);

                    return Ok("Ok");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/respuestasdetalle/registrar")]
        public async Task<IHttpActionResult> RegistrarRespuestasDetalle(List<RespuestasDetalleViewModel> RespuestasDetalle, int respuestaId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    foreach(var detalle in RespuestasDetalle)
                    {
                        var RespuestaDetalle = new RespuestaDetalle()
                        {
                            RespuestaId = respuestaId,
                            PreguntaId = detalle.PreguntaId,
                            PreguntaOpcionesId = detalle.PreguntaOpcionesId,
                            RespuestaAlfanumerica = detalle.RespuestaAlfanumerica,
                            RespuestaNumerica = detalle.RespuestaNumerica,
                            CreatedBy = detalle.Usuario,
                            CreatedDate = DateTime.Now
                        };
                        db.RespuestaDetalle.Add(RespuestaDetalle);
                    }
                   
                   
                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}