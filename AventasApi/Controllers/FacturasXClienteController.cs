using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class FacturasXClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [Route("api/FacturasXCliente/facturasXVencer{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetFacturasXVencer(string id)
        {


            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            var user = new { UserAccount = "gmonrroy" };

            var facturas = context.FacturasxCliente.Where(facCli => facCli.CodigoCliente == id).ToList();
            var FechaLimite = DateTime.Today;
            FechaLimite.AddDays(1);
            var FechaLimiteFuturo = FechaLimite.AddDays(30);
            var facturasVencidas = facturas.Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite);
            var facturasXVencer = facturas.Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo);
            var estado = new
            {
                NumeroFacturasVencidas = facturasVencidas.Count(),
                MontoFacturasVencidas = facturasVencidas.Sum(faccli => faccli.Saldo),
                NumeroFacturasXVencer = facturasXVencer.Count(),
                MontoFacturasXVencer = facturasXVencer.Sum(faccli => faccli.Saldo)
            };

            return Ok(estado);

        }
        [Route("api/FacturasXCliente/{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetFacturas(string id)
        {


            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            var user = new { UserAccount = "gmonrroy" };

            var facturas = context.FacturasxCliente.Where(facCli => facCli.CodigoCliente == id ).OrderBy(facCli=> facCli.FechaVencimiento).Select(facCli=> new FacturasXClienteViewModel
            {
                IdFactura = facCli.IdFactura,
                Factura = facCli.Factura,
                CodigoCliente = facCli.CodigoCliente,
                EmpresaId = facCli.EmpresaId,
                IdMoneda = facCli.IdMoneda,
                Tipo = facCli.Tipo,
                FechaFactura = facCli.FechaFactura,
                FechaVencimiento = facCli.FechaVencimiento,
                FechaMaxDescuento = facCli.FechaMaxDescuento,
                TotalFactura = facCli.TotalFactura,
                Saldo = facCli.Saldo,
                PendienteFactura = facCli.PendienteFactura,
                Descuento = facCli.Descuento,
                FacturaStatus = facCli.FacturaStatus,
                NumeroPagos = facCli.NumeroPagos,
                Referencia = facCli.Referencia,
                IdLinea = facCli.IdLinea,
                LineaString = facCli.MaestroLinea.Linea,
                IdTipoPedido = facCli.IdTipoPedido,
                TipoPedidoString = facCli.TiposdePedido.TipoPedido
            }).ToList();
            
            return Ok(facturas);

        }
    }
}
