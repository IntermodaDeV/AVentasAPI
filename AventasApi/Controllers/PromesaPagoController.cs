using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using DBData.Database;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    public class PromesaPagoController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public PromesaPagoController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpPost]
        [Route("~/api/promesaPago/crear")]
        public async Task<IHttpActionResult> CrearPromesaPago([FromBody] PromesaPagoModel datos)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var promesasBD = ctx.PromesaPago.Where(x => x.IdAsignacionXAsesor == datos.IdAsignacionXAsesor).ToList();
                    if(promesasBD.Count() > 0)
                    {
                        return BadRequest("Ya existe una promesa de pago registrada para la visita.");
                    }
                    var nuevaPromesa = new PromesaPago() {
                        IdAsignacionXAsesor = datos.IdAsignacionXAsesor, 
                        FechaPromesa = datos.FechaPromesa, 
                        Valor = datos.Valor,
                        UsuarioCrea = usuario.Id,
                        FechaCrea = DateTime.Now 
                    };
                    ctx.PromesaPago.Add(nuevaPromesa);
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
