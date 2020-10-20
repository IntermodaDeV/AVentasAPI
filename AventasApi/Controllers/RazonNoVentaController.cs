using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models;

namespace AventasApi.Controllers
{
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
    }
}
