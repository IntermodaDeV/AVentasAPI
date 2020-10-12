using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using AventasApi.Filters;
using DBData.Database;
using AventasApi.Models;
using AventasApi.Models.Authentication;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
//using IMS.Tokens.Services;

namespace AventasApi.Controllers
{
    //[Auth]
    public class BitacoraVisitasClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public BitacoraVisitasClienteController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            try
            {
                BitacoraVisitasClienteViewModel bitacora = context.BitacoraVisitasCliente.Select(bit => new BitacoraVisitasClienteViewModel
                {
                    IdBitacoraVisitaCliente = bit.IdBitacoraVisitaCliente,
                    IdAsignacionxAsesor = bit.IdAsignacionxAsesor,
                    Fecha = bit.Fecha,
                    IdRazonNoVentaTipo = bit.IdRazonNoVentaTipo,
                    IdRazonNoVentaCausa = bit.IdRazonNoVentaCausa,
                    CodigoCliente = bit.CodigoCliente,
                    CodigoAsesor = bit.CodigoAsesor,
                }).FirstOrDefault(bit => bit.IdAsignacionxAsesor == id);

                return Ok(bitacora);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody]  BitacoraVisitasClienteViewModel bitacoraVisitasCliente)
        {
            try
            {
                //var user =new{UserAccount = "gmonrroy"};
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
                string codigoAsesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount)?.CodigoAsesor;
                var bitacora = context.BitacoraVisitasCliente.FirstOrDefault(bit => bitacoraVisitasCliente.IdAsignacionxAsesor != null && bit.IdAsignacionxAsesor == bitacoraVisitasCliente.IdAsignacionxAsesor);
                if (bitacora == null)
                {

                    BitacoraVisitasCliente bitacoraVisitaCliente = new BitacoraVisitasCliente
                    {
                        IdBitacoraVisitaCliente = bitacoraVisitasCliente.IdBitacoraVisitaCliente,
                        Fecha = bitacoraVisitasCliente.Fecha,
                        IdAsignacionxAsesor = bitacoraVisitasCliente.IdAsignacionxAsesor,
                        IdRazonNoVentaTipo = bitacoraVisitasCliente.IdRazonNoVentaTipo,
                        IdRazonNoVentaCausa = bitacoraVisitasCliente.IdRazonNoVentaCausa,
                        CodigoCliente = bitacoraVisitasCliente.CodigoCliente,
                        CodigoAsesor = codigoAsesor,
                        Observacion = bitacoraVisitasCliente.Observacion
                    };
                    context.BitacoraVisitasCliente.Add(bitacoraVisitaCliente);
                    context.SaveChanges();
                }
                else
                {
                    bitacora.Fecha = bitacoraVisitasCliente.Fecha;
                    bitacora.IdRazonNoVentaTipo = bitacoraVisitasCliente.IdRazonNoVentaTipo;
                    bitacora.IdRazonNoVentaCausa = bitacoraVisitasCliente.IdRazonNoVentaCausa;
                    bitacora.CodigoCliente = bitacoraVisitasCliente.CodigoCliente;
                    bitacora.CodigoAsesor = codigoAsesor;
                    context.SaveChanges();
                }

                var asignacion = context.AsignacionxAsesor.FirstOrDefault(x => x.IdAsignacionxAsesor == bitacoraVisitasCliente.IdAsignacionxAsesor);
                asignacion.BloqueoCheckin = true;
                asignacion.Cancelada = true;
                context.SaveChanges();

                return StatusCode(HttpStatusCode.NoContent);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
