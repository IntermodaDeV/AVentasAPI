using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class ConfiguracionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public async Task<IHttpActionResult> Get()
        {
            var configuraciones = context.Configuraciones.ToDictionary(conf => conf.CodigoConfiguracion,
                conf => conf.Valor
            );
            return Ok(configuraciones);
        }
    }
}
