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
    public class PreguntaController : ApiController
    {

        ///---------------PREGUNTAS
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
                       //NombreSeccion = x.SeccionesEncuesta.Nombre,
                       TipoIngresoId = x.TipoIngresoId,
                       GrupoOpcionesId = x.GrupoOpcionesId,
                       Nombre = x.Nombre,
                       Descripcion = x.Descripcion,
                       Obligatorio = x.Obligatorio,
                       RespuestaObligatorio = x.RespuestaObligatorio,
                       Status = x.Status,
                       PreguntaOpciones = ctx.PreguntasOpciones.Where(p => p.PreguntaId == x.Id && p.Status == true).Select(p => new PreguntasOpcionesViewModel
                       {
                           Id = p.Id,
                           GrupoOpcionesDetalleId = p.GrupoOpcionesDetalleId,
                           GrupoOpcionesDetalle = p.GrupoOpcionesDetalle.Nombre,
                           PreguntaId = p.PreguntaId,
                           Status = p.Status
                       })
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
                using (var db = new AVentasEntities())
                {

                    List<PreguntasViewModel> Preguntas = new List<PreguntasViewModel>();
                    Preguntas.Add(preguntasEncuesta);
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
                    db.Preguntas.Add(PreguntasEncuesta);
                    var result = db.SaveChanges();

                    var preguntaId = await db.Preguntas.OrderByDescending(p => p.Id).Select(p => p.Id).FirstOrDefaultAsync();
                    if (preguntasEncuesta.GrupoOpcionesDetalle.Count() > 0)
                    {
                        _= RegistrarPreguntasOpciones(preguntasEncuesta.GrupoOpcionesDetalle, preguntaId, preguntasEncuesta.Usuario, preguntasEncuesta.GrupoOpcionesId);
                    }
                    return Ok("Ok");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/preguntaOpciones/registrar")]
        public async Task<IHttpActionResult> RegistrarPreguntasOpciones(List<int> preguntaOpciones, int preguntaId, string usuario , int? GrupoOpcionesId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpcionesDetalle = await ctx.GrupoOpcionesDetalle.Where(g => g.GrupoOpcionesId == GrupoOpcionesId && preguntaOpciones.Contains(g.Id)).Select(g => g.Id).ToListAsync();
                    foreach (var opcion in grupoOpcionesDetalle)
                    {
                        var PreguntasOpciones = new PreguntasOpciones()
                        {
                            PreguntaId = preguntaId,
                            GrupoOpcionesDetalleId = opcion,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.PreguntasOpciones.Add(PreguntasOpciones);
                    }
                   
                   
                    var result = ctx.SaveChanges();
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
        public async Task<IHttpActionResult> ModificarPregunta([FromBody] PreguntasViewModel preguntasEncuesta)
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


                    if (preguntasEncuesta.GrupoOpcionesDetalle.Count() > 0 && preguntasEncuesta.RequiereOpciones == true)
                    {
                        _ = ModificarPreguntasOpciones(preguntasEncuesta.GrupoOpcionesDetalle, PreguntasBD.Id, preguntasEncuesta.Usuario, preguntasEncuesta.GrupoOpcionesId);
                    }
                    else
                    {
                        var preguntasOpciones = ctx.PreguntasOpciones.Where(p => p.PreguntaId == PreguntasBD.Id).ToList();
                        foreach (var pregunta in preguntasOpciones)
                        {
                            pregunta.Status = false;
                            pregunta.ModifiedBy = preguntasEncuesta.Usuario;
                            pregunta.ModifiedDate = DateTime.Now;
                        }
                    }
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
        [Route("~/api/preguntaOpciones/registrar")]
        public async Task<IHttpActionResult> ModificarPreguntasOpciones(List<int> preguntaOpciones, int preguntaId, string usuario, int? GrupoOpcionesId)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var grupoOpcionesDetalle = await db.GrupoOpcionesDetalle.Where(g => g.GrupoOpcionesId == GrupoOpcionesId && preguntaOpciones.Contains(g.Id)).Select(g => g.Id).ToListAsync();
                    var preguntasOpciones = db.PreguntasOpciones.Where(p => p.PreguntaId == preguntaId && !grupoOpcionesDetalle.Contains(p.GrupoOpcionesDetalleId)).ToList();
                    foreach (var pregunta in preguntasOpciones)
                    {
                        pregunta.Status = false;
                        pregunta.ModifiedBy = usuario;
                        pregunta.ModifiedDate = DateTime.Now;
                    }

                    foreach (var opcion in grupoOpcionesDetalle)
                    {
                        var PreguntaOpcionesDB = db.PreguntasOpciones.FirstOrDefault(p => p.PreguntaId == preguntaId && p.GrupoOpcionesDetalleId == opcion);
                        if(PreguntaOpcionesDB != null)
                        {
                            PreguntaOpcionesDB.Status = true;
                            PreguntaOpcionesDB.ModifiedBy = usuario;
                            PreguntaOpcionesDB.ModifiedDate = DateTime.Now;
                        }
                        else
                        {
                            var PreguntasOpciones = new PreguntasOpciones()
                            {
                                PreguntaId = preguntaId,
                                GrupoOpcionesDetalleId = opcion,
                                Status = true,
                                CreatedBy = usuario,
                                CreatedDate = DateTime.Now
                            };
                            db.PreguntasOpciones.Add(PreguntasOpciones);
                        }
                       
                    }
                    var result = db.SaveChanges();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/preguntas/estado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pregunta = await ctx.Preguntas.FindAsync(Id);

                    if (pregunta == null)
                    {
                        return BadRequest("No se encuentra la pregunta.");
                    }

                    pregunta.Status = !pregunta.Status;
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
        [Route("~/api/preguntas/Anidadas")]
        public async Task<IHttpActionResult> RegistrarPreguntasAnidadas([FromBody] PreguntasAnidadasViewModel preguntasEncuesta)
        {
            try
            {
                using (var db = new AVentasEntities())
                {

                    List<PreguntasAnidadasViewModel> Preguntas = new List<PreguntasAnidadasViewModel>();
                    Preguntas.Add(preguntasEncuesta);

                    var PreguntasEncuesta = new PreguntasAnidadas()
                    {
                        PreguntasOpcionesId = preguntasEncuesta.PreguntasOpcionesId,
                        TipoIngresoId = preguntasEncuesta.TipoIngresoId,
                        GrupoOpcionesId = preguntasEncuesta.GrupoOpcionesId,
                        Nombre = preguntasEncuesta.Nombre,
                        Descripcion = preguntasEncuesta.Descripcion,
                        RespuestaObligatorio = preguntasEncuesta.RespuestaObligatorio,
                        Status = preguntasEncuesta.Status,
                        CreatedBy = preguntasEncuesta.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    db.PreguntasAnidadas.Add(PreguntasEncuesta);
                    var result = db.SaveChanges();

                    var preguntaId = await db.PreguntasAnidadas.OrderByDescending(p => p.Id).Select(p => p.Id).FirstOrDefaultAsync();
                    if (preguntasEncuesta.GrupoOpcionesDetalle.Count() > 0)
                    {
                        _ = RegistrarPreguntasOpcionesAnidadas(preguntasEncuesta.GrupoOpcionesDetalle, preguntaId, preguntasEncuesta.Usuario, preguntasEncuesta.GrupoOpcionesId);
                    }
                    return Ok("Ok");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/preguntaOpciones/registrar")]
        public async Task<IHttpActionResult> RegistrarPreguntasOpcionesAnidadas(List<int> preguntaOpciones, int preguntaId, string usuario, int? GrupoOpcionesId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpcionesDetalle = await ctx.GrupoOpcionesDetalle.Where(g => g.GrupoOpcionesId == GrupoOpcionesId && preguntaOpciones.Contains(g.Id)).Select(g => g.Id).ToListAsync();
                    foreach (var opcion in grupoOpcionesDetalle)
                    {
                        var PreguntasOpcionesAnidadas = new PreguntasOpcionesAnidadas()
                        {
                            PreguntaId = preguntaId,
                            GrupoOpcionesDetalleId = opcion,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.PreguntasOpcionesAnidadas.Add(PreguntasOpcionesAnidadas);
                    }

                    var result = ctx.SaveChanges();
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