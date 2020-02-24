using AventasApi.Filters;
using AventasApi.Infrastructure;
using AventasApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
//using IMS.Tokens.Services;
using AventasApi.Models.Authentication;
using AventasApi.Models.ApiModels;
using AventasApi.GestorData;
using System;
using System.Diagnostics;
using AventasApi.Services.AsyncJobs;
using AventasApi.Services.Authentication;
//using IMS.Extensions;
namespace AventasApi.Controllers
{
    //[Auth]
    public class ClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public ClienteController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        [Route("~/api/Cliente/CuentaCorriente/{codigoCliente}")]
        [HttpGet]
        public async Task<IHttpActionResult> CuentaCorriente(string codigoCliente)
        {
            try
            {
                var cuentaCorriente = context.CuentaCorriente21(codigoCliente);
                return Ok(cuentaCorriente);

            }
            catch (Exception e)
            {

                Debug.WriteLine(e);
            }
            return BadRequest();
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetClientes()
        {
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
            //var user = new { UserAccount = "gmonrroy" };
            var FechaLimite = DateTime.Today;
            FechaLimite.AddDays(1);
            var FechaLimiteFuturo = FechaLimite.AddDays(15);
            var creditos = context.PResumenCredito().ToList();
            List<ClienteViewModel> clientesSinFiltrar =

                    context.Clientes.Where(cli => cli.ClientesxRuta.FirstOrDefault().Rutas.RutasxAsesor.FirstOrDefault().Asesores.Usuario == user.UserAccount).Select(cli => new ClienteViewModel
                    {
                        EmpresaId = cli.EmpresaId,
                        Codigo = cli.CodigoCliente,
                        Nombre = cli.Nombre,
                        Zona = cli.Zona,
                        ComunidadAutonoma = cli.ComunidadAutonoma,
                        GrupoPrecio = cli.GrupoPrecio,
                        GrupoCliente = cli.GrupoCliente,
                        Descuento = cli.Descuento,
                        Direccion = cli.Direccion,
                        Moneda = cli.IdMoneda,
                        Ruta = cli.ClientesxRuta.FirstOrDefault().Rutas.Nombre,
                        CodigoRuta = cli.ClientesxRuta.FirstOrDefault().CodigoRuta,
                        Latitud = cli.Latitud,
                        LimiteCredito = cli.LimiteCredito ?? 0,
                        CreditoDisponible = cli.CreditoDisponible ?? 0,
                        Longitud = cli.Longitud,
                        //Credito =  context.PResumenCredito().Where(resCred=> resCred.codigocliente == cli.CodigoCliente).ToList(),
                        AcuerdosXTipoPedido = cli.FacturasxCliente.GroupBy(facCli => facCli.TiposdePedido).Select(asa => new AcuerdosXTipoPedidoViewModel
                        {
                            IdTipoPedido = asa.Key.IdTipoPedido,
                            TipoPedido = asa.Key.TipoPedido,
                            AgrupaPorCuota = asa.Key.AgruparPorCuotas,
                            Acuerdos = asa.GroupBy(acu => acu.AcuerdosxCliente).Select(acu => new FacturasXAcuerdosViewModel
                            {
                                Acuerdo = acu.Key == null ? "" : acu.Key.IdAcuerdoxCliente,
                                Valor = acu.Key == null ? "0" : (acu.Key.Total??0) .ToString(),
                                Disponible = acu.Key == null ? "0" : (acu.Key.Saldo??0) .ToString(),
                                //SaldoTotal = acu.Key == null ? "0" :  acu.Key.Saldo.Value.ToString(),
                                Facturas = acu.OrderBy(facCli => facCli.FechaVencimiento).Select(facCli => new FacturasXClienteViewModel
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
                                    TipoPedidoString = facCli.TiposdePedido.TipoPedido,
                                    Cuotas = facCli.SubFacturasxCliente.Where(subFac => subFac.Saldo > 0).OrderBy(subFac => subFac.FechaVencimiento).Select(subFac => new CuotasViewModel
                                    {
                                        FechaFactura = subFac.FacturasxCliente.FechaFactura,
                                        TipoDocumento = subFac.FacturasxCliente.Tipo,
                                        IdSubFactura = subFac.IdSubFactura,
                                        IdFactura = subFac.IdFactura,
                                        Factura = subFac.Factura,
                                        CodigoCliente = subFac.CodigoCliente,
                                        EmpresaId = subFac.EmpresaId,
                                        IdMoneda = subFac.IdMoneda,
                                        IdAcuerdoxCliente = subFac.IdAcuerdoxCliente,
                                        FechaVencimiento = subFac.FechaVencimiento,
                                        FechaMaxDescuento = subFac.AcuerdosxCliente != null ? subFac.FechaMaxDescuento : subFac.FacturasxCliente.FechaMaxDescuento,
                                        FechaVencimientoDescuento = subFac.FechaVencimientoDescuento,
                                        Saldo = subFac.Saldo,
                                        SaldoDivisa = subFac.SaldoDivisa,
                                        Descuento = subFac.Descuento,
                                        PendientePago = subFac.PendientePago,
                                        Referencia = subFac.Referencia,
                                        ReferenciaFacturas = subFac.ReferenciaFacturas,
                                        ReferenciaAcuerdo = subFac.ReferenciaAcuerdo,
                                        NumeroCuota = subFac.NumeroCuota,
                                        ValorCuota = (subFac.ValorCuota > 0) ? subFac.ValorCuota : facCli.TotalFactura,
                                        ValorVencidoCuota = subFac.ValorVencidoCuota,
                                        ReferenciaCuotas = subFac.ReferenciaCuotas,
                                    }).ToList()
                                }).ToList()

                            }).ToList()
                        }).ToList(),
                        NumeroFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite),
                        MontoFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite).Sum(faccli => faccli.Saldo) ?? 0,
                        NumeroFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo),
                        MontoFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo).Sum(faccli => faccli.Saldo) ?? 0,
                        FacturacionEntrega = cli.FacturacionEntrega,
                        AcuerdosVenta = cli.AcuerdosxCliente.Select(axc => new AcuerdoVentaViewModel
                        {
                            IdAcuerdoxCliente = axc.IdAcuerdoxCliente,
                            CodigoCliente = axc.CodigoCliente,
                            IdTipoPedido = axc.IdTipoPedido,
                            IdMoneda = axc.IdMoneda,
                            EmpresaId = axc.EmpresaId,
                            Tipo = axc.Tipo,
                            TipoPago = axc.TipoPago,
                            Total = axc.Total,
                            Saldo = axc.Saldo,
                            Liberado = axc.Liberado,
                            Facturado = axc.Facturado,
                            Entregado = axc.Entregado,
                            detalleAcuerdo = context.AcuerdosxClienteDetalle.Where(axcd => axcd.IdAcuerdoxCliente == axc.IdAcuerdoxCliente).Select(axcd => new AcuerdoVentaDetalleViewModel
                            {
                                Fecha = axcd.Fecha,
                                Monto = axcd.Monto,
                                Saldo = axcd.Saldo
                            }).ToList()
                        }).ToList(),
                        CuentaCorriente = context.LimiteCreditoxCliente.Where(lcc => lcc.CodigoCliente == cli.CodigoCliente).Select(lcc => new CuentaCorrienteViewModel
                        {
                            Descripcion = lcc.Descripcion,
                            Valor = lcc.Valor ?? 0
                        }).ToList()
                    }).ToList();



            foreach (var cliente in clientesSinFiltrar)
            {
                cliente.Credito = creditos.Where(resCred => resCred.codigocliente == cliente.Codigo).ToList();
            }
            return Ok(clientesSinFiltrar);

        }



    }
}
