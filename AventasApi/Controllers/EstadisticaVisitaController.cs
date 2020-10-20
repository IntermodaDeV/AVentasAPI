using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class EstadisticaVisitaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetEstadistica(DateTime FechaInicio, DateTime FechaFin)
        {
            try
            {
                var estadistica = context.EstadisticaVisita(FechaInicio, FechaFin);
                return Ok(estadistica);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
