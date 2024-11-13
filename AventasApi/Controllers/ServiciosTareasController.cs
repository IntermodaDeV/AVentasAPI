using DBData.Database;
using AventasApi.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using AventasApi.Services.Authentication;
using System.Web.Services.Description;

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
        public IHttpActionResult ServiciosTareas([FromBody] ServicioViewModel servicioViewModel)
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


        [HttpPut]
        [Route("api/update/contactoServicio")]
        public IHttpActionResult ContactoServicio([FromBody] ContactoServicioViewModel contactoViewModel)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                using (var ctx = new AVentasEntities())
                {
                    var contacto = ctx.ContactoServicio.FirstOrDefault(a => a.Id == contactoViewModel.Id);

                    if (contacto != null)
                    {
                        contacto.IdEmpresa = contactoViewModel.IdEmpresa;
                        contacto.Telefono = contactoViewModel.Telefono;
                        contacto.Whatsapp = contactoViewModel.Whatsapp;
                        contacto.UrlQRWhatsapp = contactoViewModel.UrlQRWhatsapp;
                        contacto.Activo = contactoViewModel.Activo;

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

        [HttpGet]
        [Route("api/getEstadoSW/{codigo}")]
        public async Task<IHttpActionResult> GetEstadoSW(string codigo)
        {
            try
            {
                var servicio = await context.Servicio.Where(e => e.Codigo == codigo).Select(a => a.EstadoEjecucionSW).FirstOrDefaultAsync();

                if (servicio == null)
                {
                    return NotFound();  
                }
                return Ok(servicio);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPut]
        [Route("api/update/EstadoSW")]
        public IHttpActionResult ActualizarEstadoSW([FromBody] ServicioWEstadoViewModel servicioViewModel)
        {
            try
            {              
                using (var ctx = new AVentasEntities())
                {
                    var servicio = ctx.Servicio.FirstOrDefault(a => a.Codigo == servicioViewModel.servicio);

                    if (servicio != null)
                    {
                        servicio.EstadoEjecucionSW = servicioViewModel.estado;
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
