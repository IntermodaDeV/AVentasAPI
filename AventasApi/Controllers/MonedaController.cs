using DBData.Database;
using AventasApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class MonedaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetMonedas()
        {

            var monedas = context.MaestroMoneda.Select(mon => new MonedaViewModel
            {
                IdMoneda = mon.IdMoneda,
                Moneda = mon.Moneda
            }).ToList();
            return Ok(monedas);
        }
    }
}
