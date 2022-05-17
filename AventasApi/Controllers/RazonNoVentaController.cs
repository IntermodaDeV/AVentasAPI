using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/razonnoventa")]
    public class RazonNoVentaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public IHttpActionResult GetAsesores()
        {
            try
            {
                var razones = context.RazonNoVentaTipo.Select(razonNoVentaTipo => new RazonNoVentaTipoViewModel
                {
                    IdRazonNoVentaTipo = razonNoVentaTipo.IdRazonNoVentaTipo,
                    Tipo = razonNoVentaTipo.Tipo,
                    RazonesNoVenta = razonNoVentaTipo.RazonNoVentaCausa.Select(razonNoVentaCausa => new RazonNoVentaCausaViewModel
                    {
                        IdRazonNoVentaCausa = razonNoVentaCausa.IdRazonNoVentaCausa,
                        IdRazonNoVentaTipo = razonNoVentaCausa.IdRazonNoVentaTipo,
                        Causa = razonNoVentaCausa.Causa,
                    }).ToList()
                }).ToList();
                return Ok(razones);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("tipo")]
        public async Task<IHttpActionResult> GetTipos()
        {
            try
            {
                using(AVentasEntities context = new AVentasEntities())
                {
                    var tipos = await context.RazonNoVentaTipo.Select(x => new {id=x.IdRazonNoVentaTipo,tipo=x.Tipo }).ToListAsync();
                    return Ok(tipos);
                }
            }catch(Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("causa")]
        public async Task<IHttpActionResult> GetCausas()
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    var tipos = await context.RazonNoVentaCausa.Select(x => new { id = x.IdRazonNoVentaCausa, idRazonTipo = x.IdRazonNoVentaTipo,causa=x.Causa }).ToListAsync();
                    return Ok(tipos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }
    }
}
