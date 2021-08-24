using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class MotivosDevolucionController : ApiController
    {
        [HttpGet]
        [Route("~/api/motivosDevolucion")]
        public async Task<IHttpActionResult> ObtenerMotivosDevolucion()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var ListaEncuesta = await ctx.MotivosDevolucion.Select(x => new MotivosDevolucionViewModel
                    {
                        IdMotivoDevolucion = x.IdMotivoDevolucion,
                        CodigoMotivoDevolucion = x.CodigoMotivoDevolucion,
                        Descripcion = x.Descripcion,
                        aprobacionObligatoria = x.aprobacionObligatoria,
                        EmpresaId = x.EmpresaId,
                        Estado = x.Estado
                    }).ToListAsync();
                    return Ok(ListaEncuesta);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
           