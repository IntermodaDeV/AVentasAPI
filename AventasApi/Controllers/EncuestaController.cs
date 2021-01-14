using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class EncuestaController : ApiController
    {

        ///---------------GET Y POST DE ENCUESTAS
        [HttpGet]
        [Route("~/api/Encuesta")]
        public async Task<IHttpActionResult> ObtenerEncuestas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaEncuesta = await ctx.Encuesta.Select(x => new 
                    {
                        Id =x.Id,
                        Nombre = x.Nombre,
                        Descripcion = x.Descripcion,
                        FechaInicio = x.FechaInicio,
                        FechaFin = x.FechaFin
                    }).ToListAsync();
                    return Ok(ListaEncuesta);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Encuesta/registrar")]
        public async Task<IHttpActionResult> RegistrarEncuesta([FromBody] EncuestaViewModel encuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Encuesta = new Encuesta() {
                        Nombre = encuesta.Nombre,
                        Descripcion = encuesta.Descripcion,
                        FechaInicio = encuesta.FechaInicio,
                        FechaFin = encuesta.FechaFin,
                        CreatedBy = encuesta.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.Encuesta.Add(Encuesta);
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
        [Route("~/api/Encuesta/modificar")]
        public async Task<IHttpActionResult> ModificarEncuesta([FromBody] EncuestaViewModel encuesta)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaBD = await ctx.Encuesta.FindAsync(encuesta.Id);

                    if (EncuestaBD == null)
                    {
                        return BadRequest("No se encuentra el tipo ingreso");
                    }

                    EncuestaBD.Nombre = encuesta.Nombre;
                    EncuestaBD.Descripcion = encuesta.Descripcion;
                    EncuestaBD.FechaInicio = encuesta.FechaInicio;
                    EncuestaBD.FechaFin = encuesta.FechaFin;
                    EncuestaBD.ModifiedBy = encuesta.Usuario;
                    EncuestaBD.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        ///---------------SECCIONES ENCUESTAS -----------------------------------------------------------------------------
        [HttpGet]
        [Route("~/api/Encuesta/Secciones")]
        public async Task<IHttpActionResult> ObtenerSeccionesEncuestas()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaSecciones = await ctx.SeccionesEncuesta.Select(x => new
                    {
                        EncuestaId = x.EncuestaId,
                        Nombre = x.Nombre,
                        Titulo = x.Titulo,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Encuesta/Seccion/{encuestaId}")]
        public async Task<IHttpActionResult> ObtenerSeccionEncuesta(int encuestaId)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaSecciones = await ctx.SeccionesEncuesta.Where(e=> e.EncuestaId == encuestaId).Select(x => new
                    {
                        Id = x.Id,
                        EncuestaId = x.EncuestaId,
                        Nombre = x.Nombre,
                        Titulo = x.Titulo,
                        Descripcion = x.Descripcion,
                        Obligatorio = x.Obligatorio,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(ListaSecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Encuesta/Secciones/registrar")]
        public async Task<IHttpActionResult> RegistrarSeccionesEncuesta([FromBody] SeccionEncuestaViewModel seccion)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var seccionEncuesta = new SeccionesEncuesta()
                    {
                        EncuestaId = seccion.EncuestaId,
                        Nombre = seccion.Nombre,
                        Descripcion = seccion.Descripcion,
                        Titulo = seccion.Titulo,
                        Obligatorio = seccion.Obligatorio,
                        Status = seccion.Status,
                        CreatedBy = seccion.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.SeccionesEncuesta.Add(seccionEncuesta);
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
        [Route("~/api/secciones/modificar")]
        public async Task<IHttpActionResult> ModificarSeccionEncuesta([FromBody] SeccionEncuestaViewModel secciones)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var EncuestaBD = await ctx.SeccionesEncuesta.FindAsync(secciones.Id);

                    if (EncuestaBD == null)
                    {
                        return BadRequest("No se encuentra la seccione de encuesta");
                    }
                    EncuestaBD.EncuestaId = secciones.EncuestaId;
                    EncuestaBD.Nombre = secciones.Nombre;
                    EncuestaBD.Descripcion = secciones.Descripcion;
                    EncuestaBD.Titulo = secciones.Titulo;
                    EncuestaBD.Obligatorio = secciones.Obligatorio;
                    EncuestaBD.Status = secciones.Status;
                    EncuestaBD.ModifiedBy = secciones.Usuario;
                    EncuestaBD.ModifiedDate = DateTime.Now;
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
        [Route("~/api/secciones/estado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var seccion = await ctx.SeccionesEncuesta.FindAsync(Id);

                    if (seccion == null)
                    {
                        return BadRequest("No se encuentra la sección de encuesta.");
                    }

                    seccion.Status = !seccion.Status;
                    var result = await ctx.SaveChangesAsync();
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