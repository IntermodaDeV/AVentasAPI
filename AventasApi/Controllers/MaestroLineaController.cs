using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using System.Data.Entity;
using AventasApi.Models.Authentication;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    public class MaestroLineaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;

        public MaestroLineaController()
        {
            this.context.Database.CommandTimeout = 300;
            _authenticationAppService = new AuthenticationAppService();
        }

        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                var LineasAsignadas = await context.UsuarioLinea.Where(x => x.UsuarioId == user.Id && x.Asignada == true).Select(e => e.IdLinea).ToListAsync();

                List<LineaViewModel> lineas = await context.MaestroLinea.Where(ml => ml.Visible.Value && LineasAsignadas.Contains(ml.IdLinea)).Select(ml => new LineaViewModel
                {
                    IdLinea = ml.IdLinea,
                    Linea = ml.Linea,
                    Imagen = ml.Url_Imagen
                }).OrderByDescending(x => x.IdLinea).ToListAsync();
                return Ok(lineas);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
