using AventasApi.Models.ViewModels;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace AventasApi.Controllers
{
    public class IncidenciaController : ApiController
    {
        private readonly Cloudinary _cloudinary;
        public IncidenciaController()
        {
            var account = new Account(
                "dh6wzspfy",
                "884694477656148",
                "M74wSS1dcGm6IyK10YJYBb9QW_o");

            _cloudinary = new Cloudinary(account);
        }



        [HttpPost]
        [Route("api/incidencia/registrarIncidencia")]
        public async Task<IHttpActionResult> RegistrarIncidencia([FromBody] IncidenciaViewModel body)
        {
            try
            {
                if (body.Imagenes.Count > 0)
                {
                    IncidenciaVisita incidenciaVisita = new IncidenciaVisita
                    {
                        IdAsignacionxAsesor = body.IdAsignacionAsesor,
                        Observacion = body.Observacion,
                        IdEstadosIncidencia = body.GeneraIncidencia ? 1 : 3,
                        IdTipoIncidencia = body.IdTipoIncidencia,
                        FechaCreacion = DateTime.Now
                    };


                    using (var ctx = new AVentasEntities())
                    {
                        ctx.IncidenciaVisita.Add(incidenciaVisita);
                        ctx.SaveChanges();

                        foreach (var base64Image in body.Imagenes)
                        {
                            if (!string.IsNullOrEmpty(base64Image))
                            {
                                byte[] imageBytes = Convert.FromBase64String(base64Image);
                                using (var ms = new MemoryStream(imageBytes))
                                {
                                    var uploadParams = new ImageUploadParams()
                                    {
                                        File = new FileDescription("image", ms),
                                        Folder = "Incidencias"
                                    };

                                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                                    IncidenciaVisitaDetalle incidenciaVisitaDetalle = new IncidenciaVisitaDetalle
                                    {
                                        Fecha = DateTime.Now,
                                        IdIncidenciaVisita = incidenciaVisita.Id,
                                        Fotografia = uploadResult.Url.AbsoluteUri
                                    };

                                    ctx.IncidenciaVisitaDetalle.Add(incidenciaVisitaDetalle);
                                    ctx.SaveChanges();
                                }
                            }
                        }

                       
                    }
                }

                return Ok(new { success = true, message = "Registro guardado exitosamente" });
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/incidencia/obtenerTipoIncidencia")]
        public async Task<IHttpActionResult> ObtenerTipoIncidencia()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var tipoIncidencia = await ctx.TipoIncidencia
                              .Where(a => a.Activo == true)
                              .Select(t => new
                              {
                                  t.Id,
                                  t.Descripcion,
                                  t.Activo,
                                  t.GeneraIncidencia
                              })
                              .ToListAsync();
                    return Ok(tipoIncidencia);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/incidencia/obtenerEstadosIncidencia")]
        public async Task<IHttpActionResult> ObtenerEstadosIncidencia()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var tipoIncidencia = await ctx.EstadosIncidencia
                              .Where(a => a.Activo == true)
                              .Select(t => new
                              {
                                  t.Id,
                                  t.Estado                                
                              })
                              .ToListAsync();
                    return Ok(tipoIncidencia);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/incidencia/obtenerIncidencias")]
        public async Task<IHttpActionResult> ObtenerIncidencias()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var incidencia = await ctx.IncidenciaVisita.Select(a => new {id = a.Id, asesor = a.AsignacionxAsesor.CodigoAsesor, observacion = a.Observacion, estado = a.EstadosIncidencia.Estado, tipoIncidencia = a.TipoIncidencia.Descripcion, idEstado = a.IdEstadosIncidencia, fecha = a.FechaCreacion }).ToListAsync();
                             
                    return Ok(incidencia);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/incidencia/obtenerIncidenciaDetalle/{idIncidencia}")]
        public async Task<IHttpActionResult> ObtenerIncidenciaDetalle(int idIncidencia)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var incidenciaDetalle = await ctx.IncidenciaVisitaDetalle.Where(x => x.IdIncidenciaVisita == idIncidencia).Select(a => new {fecha = a.Fecha, fotografia = a.Fotografia }).ToListAsync();

                    return Ok(incidenciaDetalle);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPut]
        [Route("api/incidencia/actualizarIncidencia")]
        public async Task<IHttpActionResult> ActualizarIncidencia([FromBody] IncidenciaPut inc)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var incidencia = ctx.IncidenciaVisita.FirstOrDefault(a => a.Id == inc.Id);
                    incidencia.IdEstadosIncidencia = inc.IdEstado;
                    ctx.SaveChanges();

                
                }



                    return Ok(new { success = true });
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
