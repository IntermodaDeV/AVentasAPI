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
    public class TipoVisitaClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpGet]
        public async Task<IHttpActionResult> GetTiposVisitaCliente()
        {
            try
            {
                var tiposVisitaCliente = context.TipoVisitaCliente.Where(tvc => tvc.Estatus.Value).Select(tvc => new TipoVisitaClienteViewModel
                {
                    idTipoVisita = tvc.idTipoVisita,
                    Nombre = tvc.Nombre,
                    Descripcion = tvc.Descripcion,
                    Estatus = tvc.Estatus.Value
                });
                return Ok(tiposVisitaCliente);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
