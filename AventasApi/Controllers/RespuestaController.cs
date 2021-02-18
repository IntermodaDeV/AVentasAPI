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
                    if(Respuestas.RespuestasDetalle.Count() > 0)
                    {
                    List<RespuestasViewModel> RespuestasEncuestas = new List<RespuestasViewModel>();
                    RespuestasEncuestas.Add(Respuestas);

                    var usuario = db.Usuarios.FirstOrDefault(u => u.usuario == Respuestas.Usuario);
                    var RespuestaDB = await db.Respuestas.FirstOrDefaultAsync(r => r.CodigoCliente == Respuestas.CodigoCliente && r.EncuestaId == Respuestas.EncuestaId && r.UsuarioId == usuario.Id);


                    var respuestaId = 0;
                    if(RespuestaDB == null)
                    {
                        var Respuesta = new Respuestas()
                        {
                            CodigoCliente = Respuestas.CodigoCliente,
                            UsuarioId = usuario.Id,
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

                    _ = RegistrarRespuestasDetalle(Respuestas.RespuestasDetalle, respuestaId, Respuestas.Usuario);

                    return Ok("Ok");
                    }
                    return BadRequest("La encuesta esta vacia");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/respuestasdetalle/registrar")]
        public async Task<IHttpActionResult> RegistrarRespuestasDetalle(List<RespuestasDetalleViewModel> RespuestasDetalle, int respuestaId, string Usuario)
        {
                using (var db = new AVentasEntities())
                {
                   
                    foreach(var detalle in RespuestasDetalle)
                    {
                    var pregunta = db.Preguntas.Where(p => p.Id == detalle.PreguntaId).FirstOrDefault();
                        if(detalle.PreguntasOpciones != null && detalle.PreguntasOpciones.Count() > 0)
                        {
                            foreach (var det in detalle.PreguntasOpciones)
                            {
                                var RespuestaDetalle = new RespuestaDetalle()
                                {
                                    RespuestaId = respuestaId,
                                    PreguntaId = detalle.PreguntaId,
                                    PreguntaOpcionesId = Convert.ToInt32(det),
                                    RespuestaAlfanumerica = detalle.RespuestaAlfanumerica,
                                    RespuestaNumerica = detalle.RespuestaNumerica,
                                    CreatedBy = Usuario,
                                    CreatedDate = DateTime.Now
                                };
                                db.RespuestaDetalle.Add(RespuestaDetalle);
                            }
                        }
                        else if(pregunta.GrupoOpcionesId == null && detalle.PreguntasOpcionesId != null)
                        {
                            var RespuestaDetalle = new RespuestaDetalle()
                            {
                                RespuestaId = respuestaId,
                                PreguntaId = detalle.PreguntaId,
                                PreguntaOpcionesId = null,
                                RespuestaAlfanumerica = detalle.RespuestaAlfanumerica,
                                RespuestaNumerica = detalle.PreguntasOpcionesId,
                                CreatedBy = Usuario,
                                CreatedDate = DateTime.Now
                            };
                            db.RespuestaDetalle.Add(RespuestaDetalle);
                        }
                        else
                        {
                            var RespuestaDetalle = new RespuestaDetalle()
                            {
                                RespuestaId = respuestaId,
                                PreguntaId = detalle.PreguntaId,
                                PreguntaOpcionesId = detalle.PreguntasOpcionesId,
                                RespuestaAlfanumerica = detalle.RespuestaAlfanumerica,
                                RespuestaNumerica = detalle.RespuestaNumerica,
                                CreatedBy = Usuario,
                                CreatedDate = DateTime.Now
                            };
                            db.RespuestaDetalle.Add(RespuestaDetalle);
                        }
                    }
                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
        }
    }
}