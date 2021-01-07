using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class TipoIngresoController : ApiController
    {
        [HttpGet]
        [Route("~/api/TipoIngreso")]
        public async Task<IHttpActionResult> ObtenerTiposIngreso()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoIngreso = await ctx.TiposIngreso.Select(x => new
                    {
                       Id = x.Id,
                       Nombre = x.Nombre,
                       Status = x.Status,
                       RequiereGrupoOpciones = x.RequiereGrupoOpciones
                    }).ToListAsync();
                    return Ok(TipoIngreso);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("~/api/TipoIngreso/registrar")]
        public async Task<IHttpActionResult> RegistrarEncuesta([FromBody] TiposIngresosViewModel tipoingreso)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoIngreso = new TiposIngreso()
                    {
                        Nombre = tipoingreso.Nombre,
                        Status = tipoingreso.Status,
                        RequiereGrupoOpciones = tipoingreso.RequiereGrupoOpciones,
                        CreatedBy = tipoingreso.Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.TiposIngreso.Add(TipoIngreso);
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
        [Route("~/api/TipoIngreso/modificar")]
        public async Task<IHttpActionResult> ModificarTipoIngreso([FromBody] TiposIngresosViewModel tipoIngreso)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoIngresoBD = await ctx.TiposIngreso.FindAsync(tipoIngreso.Id);

                    if (TipoIngresoBD == null)
                    {
                        return BadRequest("No se encuentra el tipo ingreso");
                    }

                    TipoIngresoBD.Nombre = tipoIngreso.Nombre;
                    TipoIngresoBD.Status = tipoIngreso.Status;
                    TipoIngresoBD.RequiereGrupoOpciones = tipoIngreso.RequiereGrupoOpciones;
                    TipoIngresoBD.ModifiedBy = tipoIngreso.Usuario;
                    TipoIngresoBD.ModifiedDate = DateTime.Now;
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
        [Route("~/api/tipoIngreso/estado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoIngreso = await ctx.TiposIngreso.FindAsync(Id);

                    if (TipoIngreso == null)
                    {
                        return BadRequest("No se encuentra el tipo de ingreso.");
                    }

                    TipoIngreso.Status = !TipoIngreso.Status;
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