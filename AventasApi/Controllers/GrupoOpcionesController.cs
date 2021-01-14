using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class GrupoOpcionesController : ApiController
    {

        ///---------------GRUPO DE OPCIONES HEADER
        [HttpGet]
        [Route("~/api/GrupoOpciones")]
        public async Task<IHttpActionResult> ObtenerGrupoOpciones()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var GrupoOpciones = await ctx.GrupoOpciones.Select(x => new 
                    {
                        Id =x.Id,
                        Nombre = x.Nombre,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(GrupoOpciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/GrupoOpciones/registrar")]
        public async Task<IHttpActionResult> RegistrarGrupoOpciones([FromBody] GrupoOpcionesViewModel grupoOpciones)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var GrupoOp = new GrupoOpciones() {
                        Nombre = grupoOpciones.Nombre,
                        Status = grupoOpciones.Status,
                        CreatedBy = grupoOpciones.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.GrupoOpciones.Add(GrupoOp);
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
        [Route("~/api/GrupoOpciones/modificar")]
        public async Task<IHttpActionResult> ModificarGrupoOpciones([FromBody] GrupoOpcionesViewModel GrupoOpciones)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpcionesBD = await ctx.GrupoOpciones.FindAsync(GrupoOpciones.Id);

                    if (grupoOpcionesBD == null)
                    {
                        return BadRequest("No se encuentra el grupo de opciones");
                    }

                    grupoOpcionesBD.Nombre = GrupoOpciones.Nombre;
                    grupoOpcionesBD.Status = GrupoOpciones.Status;
                    grupoOpcionesBD.ModifiedBy = GrupoOpciones.Usuario;
                    grupoOpcionesBD.ModifiedDate = DateTime.Now;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("~/api/GrupoOpciones/estado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpciones = await ctx.GrupoOpciones.FindAsync(Id);

                    if (grupoOpciones == null)
                    {
                        return BadRequest("No se encuentra el grupo de opciones.");
                    }

                    grupoOpciones.Status = !grupoOpciones.Status;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        ///---------------GRUPO DE OPCIONES DETALLE
        [HttpGet]
        [Route("~/api/GrupoOpcionesDetalle")]
        public async Task<IHttpActionResult> ObtenerGrupoOpcionesDetalle()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpcionesDetalle = await ctx.GrupoOpcionesDetalle.Select(x => new
                    {
                        Id = x.Id,
                        GrupoOpcionesId = x.GrupoOpcionesId,
                        NombreGrupoOpciones = x.GrupoOpciones.Nombre,
                        Nombre = x.Nombre
                    }).ToListAsync();
                    return Ok(grupoOpcionesDetalle);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("~/api/GrupoOpcionesDetalle/registrar")]
        public async Task<IHttpActionResult> RegistrarGrupoOpcionesDetalle([FromBody] GrupoOpcionesDetalleViewModel grupoOpcionesDetalle)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var GrupoOpcionesDetalle = new GrupoOpcionesDetalle()
                    {
                        GrupoOpcionesId = grupoOpcionesDetalle.GrupoOpcionesId,
                        Nombre = grupoOpcionesDetalle.Nombre,
                        CreatedBy = grupoOpcionesDetalle.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.GrupoOpcionesDetalle.Add(GrupoOpcionesDetalle);
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
        [Route("~/api/GrupoOpcionesDetalle/modificar")]
        public async Task<IHttpActionResult> ModificarGrupoOpcionesDetalle([FromBody] GrupoOpcionesDetalleViewModel grupoOpcionesDetalle)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var grupoOpcionesDetalleBD = await ctx.GrupoOpcionesDetalle.FindAsync(grupoOpcionesDetalle.Id);

                    if (grupoOpcionesDetalleBD == null)
                    {
                        return BadRequest("No se encuentra el registro.");
                    }
                    grupoOpcionesDetalleBD.GrupoOpcionesId = grupoOpcionesDetalle.GrupoOpcionesId;
                    grupoOpcionesDetalleBD.Nombre = grupoOpcionesDetalle.Nombre;
                    grupoOpcionesDetalleBD.ModifiedBy = grupoOpcionesDetalle.Usuario;
                    grupoOpcionesDetalleBD.ModifiedDate = DateTime.Now;
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