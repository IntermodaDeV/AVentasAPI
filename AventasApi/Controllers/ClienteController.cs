using DBData.Database;
using AventasApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using System;
using AventasApi.Services.Authentication;
using System.Data.Entity;

namespace AventasApi.Controllers
{
    public class ClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public ClienteController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetClientes()
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                var FechaLimite = DateTime.Today;
                FechaLimite.AddDays(1);
                var FechaLimiteFuturo = FechaLimite.AddDays(15);
                var creditos = context.PResumenCredito().ToList();
                var Recibos = context.AnticiposxCliente.Where(r => r.NumPedido != null).Select(a => a.NumPedido).ToList();
                List<ClienteViewModel> clientesSinFiltrar =

                        context.Clientes.Where(cli => cli.Habilitado == true && cli.CodigoAsesor == user.UserAccount).Select(cli => new ClienteViewModel
                        {
                            EmpresaId = cli.EmpresaId,
                            NombreGrupoPrecio = context.MaestroGrupoPrecio.FirstOrDefault(m => m.GrupoPrecio == cli.GrupoPrecio).Descripcion,
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
                            GrupoImpuesto = string.IsNullOrEmpty(cli.GrupoImpuesto) ? "CLIENTES" : cli.GrupoImpuesto.ToUpper(),
                            ModoEntrega = cli.ModoEntrega,
                        //Credito =  context.PResumenCredito().Where(resCred=> resCred.codigocliente == cli.CodigoCliente).ToList(),
                        NumeroFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite),
                            MontoFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite).Sum(faccli => faccli.Saldo) ?? 0,
                            NumeroFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo),
                            MontoFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo).Sum(faccli => faccli.Saldo) ?? 0,
                            FacturacionEntrega = cli.FacturacionEntrega,
                            CuentaCorriente = context.LimiteCreditoxCliente.Where(lcc => lcc.CodigoCliente == cli.CodigoCliente).Select(lcc => new CuentaCorrienteViewModel
                            {
                                Descripcion = lcc.Descripcion,
                                Valor = lcc.Valor ?? 0
                            }).ToList(),
                            Recibo = context.AnticiposxCliente.Where(r => r.CodigoCliente == cli.CodigoCliente && r.NumPedido != null).Select(rec => new AnticiposViewModel
                            {
                                NumPedido = rec.NumPedido,
                                CodigoCliente = rec.CodigoCliente
                            }).ToList(),
                            Pedido = context.PedidosxCliente.Where(ped => ped.CodigoCliente == cli.CodigoCliente && ped.IdLinea == "BIO" && !Recibos.Contains(ped.PedidoId) && ped.PedidoId != null && ped.Sincronizado == true).Select(ped => new PedidosXClienteViewModel
                            {
                                PedidoId = ped.NumeroPedido,
                                NumeroPedido = ped.PedidoId,
                                CodigoColeccion = ped.Colecciones.CodigoColeccion,
                                NombreColeccion = ped.Colecciones.Nombre,
                                FechaEntrega = ped.FechaEntrega,
                                FechaActual = ped.Fecha,
                                TotalXPedido = ped.TotalPedido,
                                ClienteContadoId = ped.ClienteContadoId

                            }).ToList()
                        }).ToList();

                foreach (var cliente in clientesSinFiltrar)
                {
                    var acuerdos = await context.AcuerdosxCliente.Where(a => a.Desde <= DateTime.Today && a.Hasta >= DateTime.Today).AsNoTracking().Where(acue => acue.CodigoCliente == cliente.Codigo).ToListAsync();
                    cliente.AcuerdosVenta = acuerdos.Select(axc => new AcuerdoVentaViewModel
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
                        Linea = axc.IdLinea,
                        Liberado = axc.Liberado,
                        Facturado = axc.Facturado,
                        Entregado = axc.Entregado,
                        detalleAcuerdo = context.AcuerdosxClienteDetalle.Where(axcd => axcd.IdAcuerdoxCliente == axc.IdAcuerdoxCliente).Select(axcd => new AcuerdoVentaDetalleViewModel
                        {
                            Fecha = axcd.Fecha,
                            Monto = axcd.Monto,
                            Saldo = axcd.Saldo
                        }).ToList()
                    }).ToList();
                    cliente.AcuerdosXTipoPedido = context.FacturasxCliente.Where(x => x.CodigoCliente == cliente.Codigo && x.Saldo > 0).GroupBy(facCli => facCli.TiposdePedido).Select(asa => new AcuerdosXTipoPedidoViewModel
                    {
                        IdTipoPedido = asa.Key.IdTipoPedido,
                        TipoPedido = asa.Key.TipoPedido,
                        AgrupaPorCuota = asa.Key.AgruparPorCuotas,
                        Acuerdos = asa.GroupBy(acu => acu.AcuerdosxCliente).Select(acu => new FacturasXAcuerdosViewModel
                        {
                            Acuerdo = acu.Key == null ? "" : acu.Key.IdAcuerdoxCliente,
                            Valor = acu.Key == null ? "0" : (acu.Key.Total ?? 0).ToString(),
                            Disponible = acu.Key == null ? "0" : (acu.Key.Saldo ?? 0).ToString(),
                            //SaldoTotal = acu.Key == null ? "0" :  acu.Key.Saldo.Value.ToString(),
                            Facturas = acu.Where(fac => fac.Saldo > 0).OrderBy(facCli => facCli.FechaVencimiento).Select(facCli => new FacturasXClienteViewModel
                            {
                                IdFactura = facCli.IdFactura,
                                Factura = facCli.Factura,
                                NumeroFEL = facCli.NumeroFEL,
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
                                Cuotas = facCli.SubFacturasxCliente.Where(subFac => subFac.FechaMaxDescuento >= DateTime.Today ? (subFac.Saldo - subFac.Descuento) > 0 : subFac.Saldo > 0).OrderBy(subFac => subFac.FechaVencimiento).Select(subFac => new CuotasViewModel
                                {
                                    FechaFactura = subFac.FacturasxCliente.FechaFactura,
                                    TipoDocumento = subFac.FacturasxCliente.Tipo,
                                    IdSubFactura = subFac.IdSubFactura,
                                    IdFactura = subFac.IdFactura,
                                    Factura = subFac.Factura,
                                    NumeroFEL = subFac.NumeroFEL,
                                    CodigoCliente = subFac.CodigoCliente,
                                    EmpresaId = subFac.EmpresaId,
                                    IdMoneda = context.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == subFac.IdMoneda).Moneda,
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
                    }).ToList();

                    cliente.Credito = creditos.Where(resCred => resCred.codigocliente == cliente.Codigo).ToList();
                }
                return Ok(clientesSinFiltrar);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

        }

        [HttpGet]
        [Route("~/api/cliente/asignacion")]
        public async Task<IHttpActionResult> GetClientesAsignacion()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<ClienteAgendaViewModel> listaClientes = new List<ClienteAgendaViewModel>();
                    foreach (var asesor in asesoresHabilitados)
                    {
                        List<ClienteAgendaViewModel> clientes = await ctx.Clientes.Where(cli => cli.Habilitado == true && cli.CodigoAsesor == asesor).Select(cli => new ClienteAgendaViewModel
                        {
                            EmpresaId = cli.EmpresaId,
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            Zona = cli.Zona,
                            ComunidadAutonoma = cli.ComunidadAutonoma,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda,
                            Ruta = cli.ClientesxRuta.FirstOrDefault().Rutas.Nombre,
                            CodigoRuta = cli.ClientesxRuta.FirstOrDefault().CodigoRuta,
                            Latitud = cli.Latitud,
                            Longitud = cli.Longitud,
                            Asesor = cli.CodigoAsesor
                            
                        }).ToListAsync();

                        listaClientes.AddRange(clientes);
                    }
                    return Ok(listaClientes);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/cliente/agenda")]
        public async Task<IHttpActionResult> GetClientesAgenda()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var FechaLimite = DateTime.Today;
                    var FechaLimiteFuturo = FechaLimite.AddDays(15);
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<ClienteAgendaViewModel> listaClientes = new List<ClienteAgendaViewModel>();
                    
                    foreach (var asesor in asesoresHabilitados)
                    {
                        List<ClienteAgendaViewModel> clientes = await ctx.Clientes.Where(cli => cli.Habilitado == true && cli.CodigoAsesor == asesor).Select(cli => new ClienteAgendaViewModel
                        {
                            EmpresaId = cli.EmpresaId,
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            Zona = cli.Zona,
                            ComunidadAutonoma = cli.ComunidadAutonoma,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda,
                            Ruta = cli.ClientesxRuta.FirstOrDefault().Rutas.Nombre,
                            CodigoRuta = cli.ClientesxRuta.FirstOrDefault().CodigoRuta,
                            Latitud = cli.Latitud,
                            Longitud = cli.Longitud,
                            NumeroFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite),
                            MontoFacturasVencidas = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento < FechaLimite).Sum(faccli => faccli.Saldo) ?? 0,
                            NumeroFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Count(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo),
                            MontoFacturasXVencer = cli.FacturasxCliente.SelectMany(faccli => faccli.SubFacturasxCliente).Where(faccli => faccli.Saldo > 0 && faccli.FechaVencimiento > FechaLimite && faccli.FechaVencimiento < FechaLimiteFuturo).Sum(faccli => faccli.Saldo) ?? 0,
                        }).ToListAsync();

                        foreach (var cliente in clientes)
                        {
                            cliente.AcuerdosXTipoPedido = ctx.FacturasxCliente.Where(x => x.CodigoCliente == cliente.Codigo && x.Saldo > 0).GroupBy(facCli => facCli.TiposdePedido).Select(asa => new AcuerdosXTipoPedidoViewModel
                            {
                                IdTipoPedido = asa.Key.IdTipoPedido,
                                TipoPedido = asa.Key.TipoPedido,
                                AgrupaPorCuota = asa.Key.AgruparPorCuotas,
                                Acuerdos = asa.GroupBy(acu => acu.AcuerdosxCliente).Select(acu => new FacturasXAcuerdosViewModel
                                {
                                    Acuerdo = acu.Key == null ? "" : acu.Key.IdAcuerdoxCliente,
                                    Valor = acu.Key == null ? "0" : (acu.Key.Total ?? 0).ToString(),
                                    Disponible = acu.Key == null ? "0" : (acu.Key.Saldo ?? 0).ToString(),
                                    Facturas = acu.Where(fac => fac.Saldo > 0).OrderBy(facCli => facCli.FechaVencimiento).Select(facCli => new FacturasXClienteViewModel
                                    {
                                        IdFactura = facCli.IdFactura,
                                        Factura = facCli.Factura,
                                        NumeroFEL = facCli.NumeroFEL,
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
                                        Cuotas = facCli.SubFacturasxCliente.Where(subFac => subFac.FechaMaxDescuento >= DateTime.Today ? (subFac.Saldo - subFac.Descuento) > 0 : subFac.Saldo > 0).OrderBy(subFac => subFac.FechaVencimiento).Select(subFac => new CuotasViewModel
                                        {
                                            FechaFactura = subFac.FacturasxCliente.FechaFactura,
                                            TipoDocumento = subFac.FacturasxCliente.Tipo,
                                            IdSubFactura = subFac.IdSubFactura,
                                            IdFactura = subFac.IdFactura,
                                            Factura = subFac.Factura,
                                            NumeroFEL = subFac.NumeroFEL,
                                            CodigoCliente = subFac.CodigoCliente,
                                            EmpresaId = subFac.EmpresaId,
                                            IdMoneda = ctx.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == subFac.IdMoneda).Moneda,
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
                            }).ToList();
                        }
                        listaClientes.AddRange(clientes);
                    }
                    return Ok(listaClientes);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/cliente/pedido")]
        public async Task<IHttpActionResult> GetClientesPedido()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var creditos = ctx.PResumenCredito().ToList();
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<ClientePedidoViewModel> listaClientes = new List<ClientePedidoViewModel>();

                    foreach (var asesor in asesoresHabilitados)
                    {

                        List<ClientePedidoViewModel> clientes = await ctx.Clientes.Where(cli => cli.Habilitado == true && cli.CodigoAsesor == asesor).Select(cli => new ClientePedidoViewModel
                        {
                            EmpresaId = cli.EmpresaId,
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            ComunidadAutonoma = cli.ComunidadAutonoma,
                            GrupoPrecio = cli.GrupoPrecio,
                            NombreGrupoPrecio = ctx.MaestroGrupoPrecio.FirstOrDefault(m => m.GrupoPrecio == cli.GrupoPrecio).Descripcion,
                            GrupoCliente = cli.GrupoCliente,
                            Descuento = cli.Descuento,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda,
                            LimiteCredito = cli.LimiteCredito ?? 0,
                            CreditoDisponible = cli.CreditoDisponible ?? 0,
                            GrupoImpuesto = string.IsNullOrEmpty(cli.GrupoImpuesto) ? "CLIENTES" : cli.GrupoImpuesto.ToUpper(),
                            ModoEntrega = cli.ModoEntrega,
                            FacturacionEntrega = cli.FacturacionEntrega,
                            CuentaCorriente = ctx.LimiteCreditoxCliente.Where(lcc => lcc.CodigoCliente == cli.CodigoCliente).Select(lcc => new CuentaCorrienteViewModel
                            {
                                Descripcion = lcc.Descripcion,
                                Valor = lcc.Valor ?? 0
                            }).ToList()
                        }).ToListAsync();

                        foreach (var cliente in clientes)
                        {
                            var acuerdos = await ctx.AcuerdosxCliente.Where(a => a.Desde <= DateTime.Today && a.Hasta >= DateTime.Today).AsNoTracking().Where(acue => acue.CodigoCliente == cliente.Codigo).ToListAsync();
                            cliente.AcuerdosVenta = acuerdos.Select(axc => new AcuerdoVentaViewModel
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
                                Linea = axc.IdLinea,
                                Liberado = axc.Liberado,
                                Facturado = axc.Facturado,
                                Entregado = axc.Entregado,
                                detalleAcuerdo = ctx.AcuerdosxClienteDetalle.Where(axcd => axcd.IdAcuerdoxCliente == axc.IdAcuerdoxCliente).Select(axcd => new AcuerdoVentaDetalleViewModel
                                {
                                    Fecha = axcd.Fecha,
                                    Monto = axcd.Monto,
                                    Saldo = axcd.Saldo
                                }).ToList()
                            }).ToList();

                            cliente.Credito = creditos.Where(resCred => resCred.codigocliente == cliente.Codigo).ToList();
                        }

                        listaClientes.AddRange(clientes);
                    }

                    return Ok(listaClientes);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/cliente/cuenta")]
        public async Task<IHttpActionResult> GetClientesCuenta()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var FechaLimite = DateTime.Today;
                    var FechaLimiteFuturo = FechaLimite.AddDays(15);
                    var Recibos = ctx.AnticiposxCliente.Where(r => r.NumPedido != null).Select(a => a.NumPedido).ToList();

                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<ClienteViewModel> listaClientes = new List<ClienteViewModel>();

                    foreach (var asesor in asesoresHabilitados)
                    {

                        List<ClienteViewModel> clientes = await ctx.Clientes.Where(cli => cli.Habilitado == true && cli.CodigoAsesor == asesor).Select(cli => new ClienteViewModel
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
                            GrupoImpuesto = string.IsNullOrEmpty(cli.GrupoImpuesto) ? "CLIENTES" : cli.GrupoImpuesto.ToUpper(),
                            ModoEntrega = cli.ModoEntrega,
                            FacturacionEntrega = cli.FacturacionEntrega,
                            CuentaCorriente = ctx.LimiteCreditoxCliente.Where(lcc => lcc.CodigoCliente == cli.CodigoCliente).Select(lcc => new CuentaCorrienteViewModel
                            {
                                Descripcion = lcc.Descripcion,
                                Valor = lcc.Valor ?? 0
                            }).ToList(),
                            Recibo = ctx.AnticiposxCliente.Where(r => r.CodigoCliente == cli.CodigoCliente && r.NumPedido != null).Select(rec => new AnticiposViewModel
                            {
                                NumPedido = rec.NumPedido,
                                CodigoCliente = rec.CodigoCliente
                            }).ToList(),
                            Pedido = ctx.PedidosxCliente.Where(ped => ped.CodigoCliente == cli.CodigoCliente && ped.IdLinea == "BIO" && !Recibos.Contains(ped.PedidoId) && ped.PedidoId != null && ped.Sincronizado == true).Select(ped => new PedidosXClienteViewModel
                            {
                                PedidoId = ped.NumeroPedido,
                                NumeroPedido = ped.PedidoId,
                                CodigoColeccion = ped.Colecciones.CodigoColeccion,
                                NombreColeccion = ped.Colecciones.Nombre,
                                FechaEntrega = ped.FechaEntrega,
                                FechaActual = ped.Fecha,
                                TotalXPedido = ped.TotalPedido,
                                ClienteContadoId = ped.ClienteContadoId

                            }).ToList()
                        }).ToListAsync();


                        foreach (var cliente in clientes)
                        {
                            cliente.AcuerdosXTipoPedido = ctx.FacturasxCliente.Where(x => x.CodigoCliente == cliente.Codigo && x.Saldo > 0).GroupBy(facCli => facCli.TiposdePedido).Select(asa => new AcuerdosXTipoPedidoViewModel
                            {
                                IdTipoPedido = asa.Key.IdTipoPedido,
                                TipoPedido = asa.Key.TipoPedido,
                                AgrupaPorCuota = asa.Key.AgruparPorCuotas,
                                Acuerdos = asa.GroupBy(acu => acu.AcuerdosxCliente).Select(acu => new FacturasXAcuerdosViewModel
                                {
                                    Acuerdo = acu.Key == null ? "" : acu.Key.IdAcuerdoxCliente,
                                    Valor = acu.Key == null ? "0" : (acu.Key.Total ?? 0).ToString(),
                                    Disponible = acu.Key == null ? "0" : (acu.Key.Saldo ?? 0).ToString(),
                                    Facturas = acu.Where(fac => fac.Saldo > 0).OrderBy(facCli => facCli.FechaVencimiento).Select(facCli => new FacturasXClienteViewModel
                                    {
                                        IdFactura = facCli.IdFactura,
                                        Factura = facCli.Factura,
                                        NumeroFEL = facCli.NumeroFEL,
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
                                        Cuotas = facCli.SubFacturasxCliente.Where(subFac => subFac.FechaMaxDescuento >= DateTime.Today ? (subFac.Saldo - subFac.Descuento) > 0 : subFac.Saldo > 0).OrderBy(subFac => subFac.FechaVencimiento).Select(subFac => new CuotasViewModel
                                        {
                                            FechaFactura = subFac.FacturasxCliente.FechaFactura,
                                            TipoDocumento = subFac.FacturasxCliente.Tipo,
                                            IdSubFactura = subFac.IdSubFactura,
                                            IdFactura = subFac.IdFactura,
                                            Factura = subFac.Factura,
                                            NumeroFEL = subFac.NumeroFEL,
                                            CodigoCliente = subFac.CodigoCliente,
                                            EmpresaId = subFac.EmpresaId,
                                            IdMoneda = ctx.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == subFac.IdMoneda).Moneda,
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
                            }).ToList();
                        }

                        listaClientes.AddRange(clientes);
                    }
                    return Ok(listaClientes);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

    }
}
