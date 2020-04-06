using DBData.Database;
using AventasApi.Models;
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
    public class TiposColeccionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetTiposColeccion()
        {
            List<TiposdeColeccionViewModel> tiposColecciones = context.TiposdeColeccion.Select(tipCol => new TiposdeColeccionViewModel
            {
                ColeccionTipo = tipCol.ColeccionTipo,
                Descripcion = tipCol.Descripcion,
                Icono = tipCol.Icono
            }).ToList();
            return Ok(tiposColecciones);
        }
    }
}
