using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class MaestroLineaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public MaestroLineaController()
        {
            this.context.Database.CommandTimeout = 300;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()
        {
            List<LineaViewModel> lineas = context.MaestroLinea.Where(ml=> ml.Visible.Value).Select(ml => new LineaViewModel
            {
                IdLinea= ml.IdLinea,
                Linea =  ml.Linea,
                Imagen = ml.Url_Imagen
            }).ToList();
            return Ok(lineas);
        }
    }
}
