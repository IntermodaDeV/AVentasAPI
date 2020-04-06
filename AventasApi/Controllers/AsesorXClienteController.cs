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
    public class AsesorXClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetAsesores()
        {
            return Ok(context.RutasxAsesor.SelectMany(rutAse => rutAse.Rutas.ClientesxRuta).Select(cliRut => new AsesorXCliente
            {
                CodigoAsesor = cliRut.Rutas.RutasxAsesor.FirstOrDefault().CodigoAsesor,
                ClienteId = cliRut.CodigoCliente
            }).ToList());
        }
    }
    public class AsesorXCliente
    {
        public string ClienteId;
        public string CodigoAsesor { get; set; }

    }
}
