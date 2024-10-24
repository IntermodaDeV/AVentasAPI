using DBData.Database;
using AventasApi.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    public class ServiciosTareasController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public ServiciosTareasController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        [Route("api/getServiciosTareas/getSevicios")]
        public async Task<IHttpActionResult> GetServiciosTareas()
        {
            try
            {
                var servicios = await context.Servicio.ToListAsync();
                return Ok(servicios);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPut]
        [Route("api/update/ServiciosTareas")]
        public IHttpActionResult ActualizarIncidencia([FromBody] ServicioViewModel servicioViewModel)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                using (var ctx = new AVentasEntities())
                {
                    var servicio = ctx.Servicio.FirstOrDefault(a => a.Id == servicioViewModel.Id);

                    if (servicio != null)
                    {
                        servicio.ProximaEjecucionEnvio = servicioViewModel.ProximaEjecucionEnvio;
                        servicio.ProximaEjecucionGenerarArchivo = servicioViewModel.ProximaEjecucionGenerarArchivo;
                        servicio.ReIniciar = servicioViewModel.ReIniciar;
                        servicio.ReIniciarGenerarcionArchivo = servicioViewModel.ReIniciarGenerarcionArchivo;
                        servicio.FechaModificacion = DateTime.Now;
                        servicio.UsuarioModificion = user.UserAccount;
                        ctx.SaveChanges();
                    }
              
                   

                }
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
