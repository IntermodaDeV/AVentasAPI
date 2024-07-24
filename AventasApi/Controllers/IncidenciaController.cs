using AventasApi.Models.ViewModels;
using AventasApi.Services.CloudinaryService;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace AventasApi.Controllers
{
    public class IncidenciaController : ApiController
    {
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

                        var cloud = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "Cloud_Cloudinary").Valor;
                        var apiKey = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "ApiKey_Cloudinary").Valor;
                        var apiSecret = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "ApiSecret_Cloudinary").Valor;

                        CloudinaryFunciones cloudinaryFunciones = new CloudinaryFunciones(cloud, apiKey, apiSecret);

                        foreach (var base64Image in body.Imagenes)
                        {
                            if (!string.IsNullOrEmpty(base64Image))
                            {

                                var url = await cloudinaryFunciones.Upload(base64Image);

                                if (url != null)
                                {
                                    IncidenciaVisitaDetalle incidenciaVisitaDetalle = new IncidenciaVisitaDetalle
                                    {
                                        Fecha = DateTime.Now,
                                        IdIncidenciaVisita = incidenciaVisita.Id,
                                        Fotografia = url
                                    };

                                    ctx.IncidenciaVisitaDetalle.Add(incidenciaVisitaDetalle);
                                }
                            }
                        }
                        ctx.SaveChanges();
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
        [Route("~/api/incidencia/obtenerIncidencias/{fechaInicio}/{fechaFin}")]
        public async Task<IHttpActionResult> ObtenerIncidencias(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                  

                    var incidencia = await ctx.IncidenciaVisita
                                     .Where(a => a.FechaCreacion >= fechaInicio && a.FechaCreacion <= fechaFin)
                                     .Select(a => new { id = a.Id, cliente = a.AsignacionxAsesor.Clientes.Nombre , asesor = a.AsignacionxAsesor.CodigoAsesor, observacion = a.Observacion, estado = a.EstadosIncidencia.Estado, tipoIncidencia = a.TipoIncidencia.Descripcion, idEstado = a.IdEstadosIncidencia, fecha = a.FechaCreacion, color = a.EstadosIncidencia.color })
                                     .OrderBy(s=> s.fecha)
                                     .ToListAsync();

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
                    var incidenciaDetalle = await ctx.IncidenciaVisitaDetalle.Where(x => x.IdIncidenciaVisita == idIncidencia).Select(a => new { fecha = a.Fecha, fotografia = a.Fotografia }).ToListAsync();

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
        public IHttpActionResult ActualizarIncidencia([FromBody] IncidenciaPut inc)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var incidencia = ctx.IncidenciaVisita.FirstOrDefault(a => a.Id == inc.Id);
                    incidencia.IdEstadosIncidencia = inc.IdEstado;
                    incidencia.FechaModificacion = DateTime.Now;
                    ctx.SaveChanges();

                }
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpDelete]
        [Route("~/api/incidencia/eliminarImagenes")]
        public async Task<IHttpActionResult> EliminarImagenes()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {

                    var cloud = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "Cloud_Cloudinary").Valor;
                    var apiKey = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "ApiKey_Cloudinary").Valor;
                    var apiSecret = ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "ApiSecret_Cloudinary").Valor;
                    var dias = Convert.ToInt32(ctx.Configuraciones.FirstOrDefault(a => a.CodigoConfiguracion == "Dias_Cloudinary").Valor);

                    var A = await ctx.IncidenciaVisitaDetalle.Where(a => a.Eliminada == false && a.IncidenciaVisita.IdEstadosIncidencia == 3 && DbFunctions.DiffDays(a.IncidenciaVisita.FechaModificacion, DateTime.Now) > dias).ToListAsync();

                    CloudinaryFunciones cloudinaryFunciones = new CloudinaryFunciones(cloud, apiKey, apiSecret);

                    foreach (var item in A)
                    {
                        var eliminada = await cloudinaryFunciones.DeleteImageByUrl(item.Fotografia);

                        if (eliminada)
                        {
                            item.Eliminada = true;
                            ctx.SaveChanges();
                        }
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("~/api/incidencia/obtenerCantidadFotosPermitidas")]
        public IHttpActionResult ObtenerCantidadFotosPermitidas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var cantidad = Convert.ToInt32(ctx.Configuraciones.FirstOrDefaultAsync(a => a.CodigoConfiguracion == "CantImagenes").Result.Valor);
                   
                    return Ok(new { cantidad = cantidad }); ;
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/incidencia/incidenciaReportadaVisita/{id}")]
        public async Task<IHttpActionResult> IncidenciaReportadaVisita(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    bool reportada = await ctx.IncidenciaVisita
                      .Where(a => a.IdAsignacionxAsesor == id)
                      .AnyAsync();

                    return Ok(new { reportada = reportada });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
