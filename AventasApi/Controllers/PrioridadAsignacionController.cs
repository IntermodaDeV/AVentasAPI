using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class PrioridadAsignacionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpGet]
        public async Task<IHttpActionResult> GetPrioridadAsignacion()
        {
            var prioridades = context.PrioridadAsignacion.Where(pri => pri.Estatus.Value).Select(pri => new PrioridadAsignacionViewModel
            {
                idPrioridad = pri.idPrioridad,
                NombrePrioridad = pri.NombrePrioridad,
                Estatus = pri.Estatus.Value,
                ColorBorde = pri.ColorBorde,
                ColorRelleno = pri.ColorRelleno
            });
            return Ok(prioridades);
        }
    }
}
