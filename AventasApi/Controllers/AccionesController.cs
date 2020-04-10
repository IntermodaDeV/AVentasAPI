using AventasApi.Filters;
//using DBData.Database;
using AventasApi.Models.Authentication;
//using IMS.Tokens.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class AccionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var acciones = context.Acciones.Select(acc => new AccionesViewModel
            {
                IdAccion = acc.IdAccion,
                Accion = acc.Accion,
                UrlRedirect = acc.UrlRedirect,
                Estado = acc.Estado,
                Orden = acc.Orden
            }).ToList();
            return Ok(acciones);
        }
    }
}
