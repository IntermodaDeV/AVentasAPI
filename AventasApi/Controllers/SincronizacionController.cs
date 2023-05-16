using AventasApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/sincronizar")]
    public class SincronizacionController : ApiController
    {
        [HttpGet]
        [Route("asesores")]
        public async Task<IHttpActionResult> SincronizarAsesores()
        {
            try
            {
                await new SyncAsesores().SincronizacionAsesores();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }          
        }


        [HttpGet]
        [Route("rutasAsesores")]
        public async Task<IHttpActionResult> SincronizarRutasAsesores()
        {
            try
            {
                await new SyncRutas().SincronizarRurasAsesor();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        [HttpGet]
        [Route("clientes/{codigoAsesor}")]
        public async Task<IHttpActionResult> SincronizarClientes(string codigoAsesor)
        {
            try
            {
                await new SyncClientes().SincronizacionClientes(codigoAsesor);
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }

        }



    }
}
