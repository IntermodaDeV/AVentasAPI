using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using System.Data.Entity;

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
            try
            {
                List<LineaViewModel> lineas = await context.MaestroLinea.Where(ml => ml.Visible.Value).Select(ml => new LineaViewModel
                {
                    IdLinea = ml.IdLinea,
                    Linea = ml.Linea,
                    Imagen = ml.Url_Imagen
                }).ToListAsync();
                return Ok(lineas);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
