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
    public class ConfiguracionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var configuraciones = context.Configuraciones.ToDictionary(conf => conf.CodigoConfiguracion,
                    conf => conf.Valor
                );
                return Ok(configuraciones);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/configuraciones/conexion")]
        public IHttpActionResult VerificarConexion()
        {
            return Ok();
        }
    }
}
