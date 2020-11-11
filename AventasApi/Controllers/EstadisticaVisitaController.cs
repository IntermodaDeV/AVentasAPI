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

        [HttpGet]
        [Route("~/api/estadisticavisita/mes")]
        public IHttpActionResult GetVisitasPorMes()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var visitas = ctx.SP_VISITASPORMES().Select(x => new
                    {
                        MES = Convertir(x.M, x.ANIO.Value),
                        VISITAS = x.VISITAS
                    }).ToList();         
                    return Ok(visitas);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        private string Convertir(int mes,int anio)
        {
            switch (mes)
            {
                case 1:
                    return $"Enero-{anio}";
                case 2:
                    return $"Febrero-{anio}";
                case 3:
                    return $"Marzo-{anio}";
                case 4:
                    return $"Abril-{anio}";
                case 5:
                    return $"Mayo-{anio}";
                case 6:
                    return $"Junio-{anio}";
                case 7:
                    return $"Julio-{anio}";
                case 8:
                    return $"Agosto-{anio}";
                case 9:
                    return $"Septiembre-{anio}";
                case 10:
                    return $"Octubre-{anio}";
                case 11:
                    return $"Noviembre-{anio}";
                default:
                    return $"Diciembre-{anio}";
            }
        }
    }
}
