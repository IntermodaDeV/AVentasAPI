using AventasApi.Models;
using AventasApi.Services.Authentication;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/documentospendientes")]
    public class DocumentosPendientesController : ApiController
    {

        private readonly AuthenticationAppService _authenticationAppService;

        public DocumentosPendientesController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        [HttpGet]
        [Route("facturas")]
        public async Task<IHttpActionResult> ObtenerFacturasPendientes()
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<string> asesoresHabilitados = new List<string>();
                    List<string> clientes = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                        clientes = await ctx.Clientes.Where(x => asesoresHabilitados.Contains(x.CodigoAsesor)).Select(x => x.CodigoCliente).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                        clientes = await ctx.Clientes.Where(x => asesoresHabilitados.Contains(x.CodigoAsesor)).Select(x => x.CodigoCliente).ToListAsync();
                    }

                    List<FacturaPendiente> listaFacturasPendientes = await ctx.DocumentosTransitoxFactura
                        .Where(x => clientes.Contains(x.CodigoCliente))
                        .Select(c => new FacturaPendiente() {
                            CodigoCliente=c.CodigoCliente,
                            Valor=c.Valor,
                            Tipo=c.Tipo,
                            Moneda=c.IdMoneda,
                            FechaDocumento=c.FechaCreacion,
                            Factura=c.Factura,
                            NumeroDocumento=c.NumeroDocumento,
                            Estado=c.Estado,
                            CreadoPor=c.CreadoPor,
                            ReferenciaAx=c.TablaId,
                            IdentificadorAx=c.Referencia,
                            NumeroFel=c.NumeroFEL
                        })
                        .ToListAsync();

                    return Ok(listaFacturasPendientes);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
