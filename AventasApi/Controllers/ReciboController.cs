//using AventasApi.Enviroments;
using AventasApi.Models;
//using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;
using AventasApi.Services.AsyncJobs;
using AventasApi.Services.Authentication;
using AventasApi.Utils;
using DBData.Database;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AventasApi.Controllers
{
    public class ReciboController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        private SyncCuentaCorriente syncCuentaCorriente;
        public ReciboController()
        {
            _authenticationAppService = new AuthenticationAppService();
            syncCuentaCorriente = new SyncCuentaCorriente();
        }

        private bool EnLinea(string empresa, string asesor)
        {
            var client = new RestClient(Enviroment.CRMWebServiceURLApi);
            client.Authenticator = new RestSharp.Authenticators.NtlmAuthenticator();
            var request = new RestRequest($"asesor/{empresa}/{asesor}", Method.GET);
            client.Timeout = 6000;
            IRestResponse<List<AsesorApiModel>> respuesta = client.Execute<List<AsesorApiModel>>(request);

            return respuesta.IsSuccessful;
        }

        [HttpGet]
        [Route("~/api/recibos/correlativo")]
        public async Task<IHttpActionResult> GetCorrelativo()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount);
                    int numeroCorelativo = asesor.CorrelativoRecibos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";


                    return Ok(numeroReferencia);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("~/api/Recibo/{asesor}/{FechaInicio}/{FechaFin}")]
        public async Task<IHttpActionResult> Get(string Asesor, DateTime FechaInicio, DateTime FechaFin)
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
                        asesoresHabilitados = await context.Asesores.Where(x => x.CodigoAsesor == Asesor).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId)).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }

                    List<RecibosxClienteViewModel> ListaRecibos = new List<RecibosxClienteViewModel>();
                    foreach (var asesor in asesoresHabilitados)
                    {
                    var Recibos = context.RecibosxCliente.Where(r => r.CodigoAsesor == asesor && r.Fecha >= FechaInicio && r.Fecha < FechaFin).Select(rec => new RecibosxClienteViewModel
                    {
                        Anticipo=false,
                        NombreAsesor = context.Asesores.FirstOrDefault(x=>x.CodigoAsesor==rec.CodigoAsesor).Nombre,
                        Asesor = rec.CodigoAsesor,
                        NumeroRecibo = rec.NumeroRecibo,
                        CodigoCliente = rec.CodigoCliente,
                        Fecha = rec.Fecha,
                        IdTipoPago = rec.IdTipoPago,
                        Referencia = rec.Referencia,
                        FechaPago = rec.FechaCheque,
                        IdBanco = rec.IdBanco,
                        Valor = rec.Valor,
                        IdMoneda = context.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == rec.IdMoneda).Moneda,
                        Sincronizado = rec.Sincronizado,
                        CodigoAsesor = rec.CodigoAsesor,
                        IdFactura = rec.IdFactura,
                        Longitude = rec.Longitude,
                        Latitude = rec.Latitude,
                        DescripcionBanco = context.Bancos.Where(banco => banco.IdBanco == rec.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                        Descuento = rec.Descuento,
                        Cliente = context.Clientes.Where(cli => cli.CodigoCliente == rec.CodigoCliente).Select(cli => new ClienteViewModel
                        {
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda
                        }).FirstOrDefault(),
                        TipoPago = context.TiposdePago.Where(tp => tp.IdTipoPago == rec.IdTipoPago).Select(tp => new TipoPagoViewModel
                        {
                            IdTipoPago = tp.IdTipoPago,
                            Codigo = tp.Codigo,
                            Descripcion = tp.Descripcion,
                            Tipo = tp.Tipo,
                            EmpresaId = tp.EmpresaId,
                            TiposdePagoDetalle = tp.TiposdePagoDetalle.Where(d => d.CodigoDetalle == rec.SpecPago).Select(pd => new TipoPagoDetalleViewModel
                            { 
                                Codigo = pd.Codigo,
                                CodigoDetalle = pd.CodigoDetalle,
                                Descripcion = pd.Descripcion
                            }).ToList(),
                        }).FirstOrDefault(),
                        DetalleRecibo = rec.RecibosDetalle.Select(recDet =>
                        recDet.SubFacturasxCliente != null ?
                        new RecibosDetalleViewModel
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            Factura = recDet.SubFacturasxCliente.Factura,
                            NumeroFel = recDet.SubFacturasxCliente.NumeroFEL,
                            FechaFactura = recDet.SubFacturasxCliente.FacturasxCliente.FechaFactura,
                            Tipo = rec.FacturasxCliente.Tipo,
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = recDet.IdSubFactura,
                            Valor = recDet.Valor,
                            ValorFactura = recDet.SubFacturasxCliente.FacturasxCliente.TotalFactura,
                            ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                            Descuento = recDet.Descuento,
                            EsAbono = recDet.EsAbono,
                            DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                        } : new RecibosDetalleViewModel
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            Factura = "SALDO_FAVOR",
                            NumeroFel = "",
                            FechaFactura = null,
                            Tipo = "Pago",
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = null,
                            Valor = recDet.Valor,
                            ValorFactura = 0,
                            ValorSinDescuento = recDet.Valor,
                            Descuento = 0,
                            EsAbono = true,
                            DiasVencimiento = 0,
                        }
                        ).ToList()
                    }).ToList();
                    
                    var anticiposXAsesor = context.AnticiposxCliente.Where(recCli => recCli.CodigoAsesor == asesor).Select(ant => new RecibosxClienteViewModel
                    {
                        Anticipo=true,
                        NombreAsesor = context.Asesores.FirstOrDefault(x => x.CodigoAsesor == ant.CodigoAsesor).Nombre,
                        Asesor = ant.CodigoAsesor,
                        NumeroRecibo = ant.NumeroRecibo,
                        CodigoCliente = ant.CodigoCliente,
                        Fecha = ant.Fecha,
                        IdTipoPago = ant.IdTipoPago,
                        Referencia = ant.Referencia,
                        FechaPago = ant.FechaCheque,
                        IdBanco = ant.IdBanco,
                        Valor = ant.Valor,
                        IdMoneda = context.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == ant.IdMoneda).Moneda,
                        Sincronizado = ant.Sincronizado,
                        CodigoAsesor = ant.CodigoAsesor,
                        IdFactura = 0,
                        Latitude = ant.Latitude,
                        Longitude = ant.Longitude,
                        DescripcionBanco = context.Bancos.Where(banco => banco.IdBanco == ant.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                        Descuento = 0,
                        Cliente = context.Clientes.Where(cli => cli.CodigoCliente == ant.CodigoCliente).Select(cli => new ClienteViewModel
                        {
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda
                        }).FirstOrDefault(),
                        TipoPago = context.TiposdePago.Where(tp => tp.IdTipoPago == ant.IdTipoPago).Select(tp => new TipoPagoViewModel
                        {
                            IdTipoPago = tp.IdTipoPago,
                            Codigo = tp.Codigo,
                            Descripcion = tp.Descripcion,
                            Tipo = tp.Tipo,
                            EmpresaId = tp.EmpresaId,

                        }).FirstOrDefault(),
                        Pedido = context.PedidosxCliente.Where(p => p.NumeroPedido == ant.NumPedido).Select(ped => new PedidosXClienteViewModel
                        {
                            NumeroPedido = ped.NumeroPedido,
                            ClienteContadoId = ped.ClienteContadoId
                        }).FirstOrDefault(),
                        DetalleRecibo = new List<RecibosDetalleViewModel> { new RecibosDetalleViewModel {
                    Valor = ant.Valor,
                    ValorSinDescuento = ant.Valor,
                    DiasVencimiento = 0,
                    Tipo = ant.Tipo,
                    Factura = "Anticipo",
                    Descuento=0,
                    FechaFactura=ant.Fecha
                } }
                    }).ToList();
                        ListaRecibos.AddRange(Recibos);
                        ListaRecibos.AddRange(anticiposXAsesor);

                }
                    return Ok(ListaRecibos);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("api/Recibo/Pendiente")]
        public IHttpActionResult GetPendientes()
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                var recibosXAsesor = context.RecibosxCliente.Where(recCli => recCli.CodigoAsesor == user.UserAccount && recCli.Sincronizado==false).Select(rec => new RecibosxClienteViewModel
                {
                    Anticipo=false,
                    NumeroRecibo = rec.NumeroRecibo,
                    CodigoCliente = rec.CodigoCliente,
                    Fecha = rec.Fecha,
                    IdTipoPago = rec.IdTipoPago,
                    Referencia = rec.Referencia,
                    FechaPago = rec.FechaCheque,
                    IdBanco = rec.IdBanco,
                    Valor = rec.Valor,
                    IdMoneda = context.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == rec.IdMoneda).Moneda,
                    Sincronizado = rec.Sincronizado,
                    CodigoAsesor = rec.CodigoAsesor,
                    IdFactura = rec.IdFactura,
                    Longitude = rec.Longitude,
                    Latitude = rec.Latitude,
                    DescripcionBanco = context.Bancos.Where(banco => banco.IdBanco == rec.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                    Descuento = rec.Descuento,
                    Cliente = context.Clientes.Where(cli => cli.CodigoCliente == rec.CodigoCliente).Select(cli => new ClienteViewModel
                    {
                        Codigo = cli.CodigoCliente,
                        Nombre = cli.Nombre,
                        Direccion = cli.Direccion,
                        Moneda = cli.IdMoneda
                    }).FirstOrDefault(),
                    TipoPago = context.TiposdePago.Where(tp => tp.IdTipoPago == rec.IdTipoPago).Select(tp => new TipoPagoViewModel
                    {
                        IdTipoPago = tp.IdTipoPago,
                        Codigo = tp.Codigo,
                        Descripcion = tp.Descripcion,
                        Tipo = tp.Tipo,
                        EmpresaId = tp.EmpresaId,

                    }).FirstOrDefault(),
                    DetalleRecibo = rec.RecibosDetalle.Select(recDet =>
                    recDet.SubFacturasxCliente != null ?
                    new RecibosDetalleViewModel
                    {
                        IdReciboDetalle = recDet.IdReciboDetalle,
                        Factura = recDet.SubFacturasxCliente.Factura,
                        NumeroFel = recDet.SubFacturasxCliente.NumeroFEL,
                        FechaFactura = recDet.SubFacturasxCliente.FacturasxCliente.FechaFactura,
                        Tipo = rec.FacturasxCliente.Tipo,
                        ReciboId = recDet.ReciboId,
                        IdSubFactura = recDet.IdSubFactura,
                        Valor = recDet.Valor,
                        ValorFactura = recDet.SubFacturasxCliente.FacturasxCliente.TotalFactura,
                        ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                        Descuento = recDet.Descuento,
                        EsAbono = recDet.EsAbono,
                        DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                    } : new RecibosDetalleViewModel
                    {
                        IdReciboDetalle = recDet.IdReciboDetalle,
                        Factura = "SALDO_FAVOR",
                        NumeroFel = "",
                        FechaFactura = null,
                        Tipo = "Pago",
                        ReciboId = recDet.ReciboId,
                        IdSubFactura = null,
                        Valor = recDet.Valor,
                        ValorFactura = 0,
                        ValorSinDescuento = recDet.Valor,
                        Descuento = 0,
                        EsAbono = true,
                        DiasVencimiento = 0,
                    }
                    ).ToList()
                }).ToList();
                var anticiposXAsesor = context.AnticiposxCliente.Where(recCli => recCli.CodigoAsesor == user.UserAccount && recCli.Sincronizado==false).Select(ant => new RecibosxClienteViewModel
                {
                    Anticipo=true,
                    NumeroRecibo = ant.NumeroRecibo,
                    CodigoCliente = ant.CodigoCliente,
                    Fecha = ant.Fecha,
                    IdTipoPago = ant.IdTipoPago,
                    Referencia = ant.Referencia,
                    FechaPago = ant.FechaCheque,
                    IdBanco = ant.IdBanco,
                    Valor = ant.Valor,
                    IdMoneda = context.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == ant.IdMoneda).Moneda,
                    Sincronizado = ant.Sincronizado,
                    CodigoAsesor = ant.CodigoAsesor,
                    IdFactura = 0,
                    Latitude = ant.Latitude,
                    Longitude = ant.Longitude,
                    DescripcionBanco = context.Bancos.Where(banco => banco.IdBanco == ant.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                    Descuento = 0,
                    Cliente = context.Clientes.Where(cli => cli.CodigoCliente == ant.CodigoCliente).Select(cli => new ClienteViewModel
                    {
                        Codigo = cli.CodigoCliente,
                        Nombre = cli.Nombre,
                        Direccion = cli.Direccion,
                        Moneda = cli.IdMoneda
                    }).FirstOrDefault(),
                    TipoPago = context.TiposdePago.Where(tp => tp.IdTipoPago == ant.IdTipoPago).Select(tp => new TipoPagoViewModel
                    {
                        IdTipoPago = tp.IdTipoPago,
                        Codigo = tp.Codigo,
                        Descripcion = tp.Descripcion,
                        Tipo = tp.Tipo,
                        EmpresaId = tp.EmpresaId,

                    }).FirstOrDefault(),
                    Pedido = context.PedidosxCliente.Where(p => p.NumeroPedido == ant.NumPedido).Select(ped => new PedidosXClienteViewModel
                    {
                        NumeroPedido = ped.NumeroPedido,
                        ClienteContadoId = ped.ClienteContadoId
                    }).FirstOrDefault(),
                    DetalleRecibo = new List<RecibosDetalleViewModel> { new RecibosDetalleViewModel {
                    Valor = ant.Valor,
                    ValorSinDescuento = ant.Valor,
                    DiasVencimiento = 0,
                    Tipo = ant.Tipo,
                    Factura = "Anticipo",
                    Descuento=0,
                    FechaFactura=ant.Fecha
                } }
                }).ToList();
                recibosXAsesor.AddRange(anticiposXAsesor);
                return Ok(recibosXAsesor);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("api/Recibo/Pendiente/{recibo}")]
        public async Task<IHttpActionResult> PostPendientes(string recibo)
        {
            if (EnLinea("IMHN", "hbenitez"))
            {
               using(var ctx = new AVentasEntities())
                {
                    var Recibo = ctx.RecibosxCliente.FirstOrDefault(x => x.NumeroRecibo == recibo);

                    if(Recibo == null)
                    {
                        return BadRequest("El recibo no existe.");
                    }

                    List<ReciboApiModel> ReciboSincronizar = new List<ReciboApiModel>();
                    var ReciboDetalle = ctx.RecibosDetalle.Where(s => s.RecibosxCliente.NumeroRecibo == recibo).ToList();
                    var asesor = ctx.Asesores.Where(a => a.CodigoAsesor == Recibo.CodigoAsesor).FirstOrDefault();
                    var TipoPago = ctx.TiposdePago.Where(a => a.IdTipoPago == Recibo.IdTipoPago).FirstOrDefault();
                    var Banco = ctx.Bancos.Where(a => a.IdBanco == Recibo.IdBanco).FirstOrDefault();

                    foreach (var Detalle in ReciboDetalle)
                    {
                        var Recibos = new ReciboApiModel
                        {
                            COMPANY = asesor.EmpresaId,
                            ASESOR = asesor.Usuario,
                            ASESOR_NOMBRE = asesor.Nombre,
                            ASESOR_DIARIO = asesor.CodigoAsesor,
                            RECIBO = Recibo.NumeroRecibo,
                            CLIENTE = Recibo.CodigoCliente,
                            MONEDA = Recibo.IdMoneda,
                            FECHA = Recibo.Fecha.Value.ToString("dd/MM/yyyy"),
                            DESCRIPCION = "",
                            TOTAL_RECIBO = Recibo.Valor.ToString(),
                            TOTAL_FACTURAS = ReciboDetalle.Count().ToString(),
                            TOTAL_APLICADO = Recibo.Valor.ToString(),
                            TIPO_PAGO = TipoPago.Codigo,
                            SPEC_PAGO = Recibo.SpecPago,
                            BANCO = Banco != null ? Banco.NombreBanco : "",
                            REFERENCIA = Recibo.Referencia,
                            FECHA_PAGO = Recibo.FechaCheque.Value.ToString("dd/MM/yyyy"),
                            FACTURA = Detalle.SubFacturasxCliente.Factura,
                            APLICADO = Detalle.Valor.ToString(),
                            DESCUENTO = Detalle.Descuento.ToString(),
                            REF_TRANSOPEN = Detalle.SubFacturasxCliente.Referencia,
                            ES_CONTADO = "0",
                            NUM_PEDIDO = "",

                        };
                        ReciboSincronizar.Add(Recibos);
                    }
                    return await PostReciboAx(ReciboSincronizar);
                }
            }
            else
            {
                return BadRequest("El servidor de AX no esta disponible.");
            }
        }

        [HttpPost]
        [Route("api/Recibo/Anticipo/Pendiente/{recibo}")]
        public async Task<IHttpActionResult> PostPendientesAnticipo(string recibo)
        {
            if (EnLinea("IMHN", "hbenitez"))
            {
                using (var ctx = new AVentasEntities())
                {
                    var Recibo = ctx.AnticiposxCliente.FirstOrDefault(x => x.NumeroRecibo == recibo);

                    if (Recibo == null)
                    {
                        return BadRequest("El recibo no existe.");
                    }

                    List<ReciboApiModel> ReciboSincronizar = new List<ReciboApiModel>();
                    var asesor = ctx.Asesores.Where(a => a.CodigoAsesor == Recibo.CodigoAsesor).FirstOrDefault();
                    var TipoPago = ctx.TiposdePago.Where(a => a.IdTipoPago == Recibo.IdTipoPago).FirstOrDefault();
                    var Banco = ctx.Bancos.Where(a => a.IdBanco == Recibo.IdBanco).FirstOrDefault();

                    var Recibos = new ReciboApiModel
                    {
                        COMPANY = asesor.EmpresaId,
                        ASESOR = asesor.Usuario,
                        ASESOR_NOMBRE = asesor.Nombre,
                        ASESOR_DIARIO = asesor.CodigoAsesor,
                        RECIBO = Recibo.NumeroRecibo,
                        CLIENTE = Recibo.CodigoCliente,
                        MONEDA = Recibo.IdMoneda,
                        FECHA = Recibo.Fecha.Value.ToString("dd/MM/yyyy"),
                        DESCRIPCION = "",
                        TOTAL_RECIBO = Recibo.Valor.ToString(),
                        TOTAL_FACTURAS = Recibo.Valor.ToString(),
                        TOTAL_APLICADO = Recibo.Valor.ToString(),
                        TIPO_PAGO = TipoPago.Codigo,
                        SPEC_PAGO = Recibo.SpecPago,
                        BANCO = Banco != null ? Banco.NombreBanco : "",
                        REFERENCIA = Recibo.Referencia,
                        FECHA_PAGO = Recibo.FechaCheque.Value.ToString("dd/MM/yyyy"),
                        FACTURA = "Anticipo",
                        APLICADO = Recibo.Valor.ToString(),
                        DESCUENTO = Recibo.Descuento.ToString(),
                        REF_TRANSOPEN = "",
                        ES_CONTADO = Recibo.EsContado.ToString(),
                        NUM_PEDIDO = Recibo.NumPedido,
                    };
                    ReciboSincronizar.Add(Recibos);
                

                    return await PostAnticipoAx(ReciboSincronizar);
                }
            }
            else
            {
                return BadRequest("El servidor de AX no esta disponible.");
            }
        }

        [Route("api/Recibo/Anticipo")]
        [HttpPost]
        public IHttpActionResult PostAnticipo(ReciboPostViewModel anticipoPost)
        {
            try
            {
                RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                var asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                if (anticipoPost.Pagos != null)
                {
                    var existeAnticipo = 0;
                    List<ReciboApiModel> recibos = new List<ReciboApiModel>();
                    foreach (var pag in anticipoPost.Pagos)
                    {
                        int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                        string inicialesAsesor = asesor.InicialesNombre;
                        var pago = pag;
                        int.TryParse(pago.IdBanco, out int codigobanco);

                        decimal.TryParse(pago.Valor.ToString(), out decimal valorPago);
                        var minutosConf = context.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotanteRecibo");
                        double minutosValue = 1;

                        if (minutosConf != null)
                        {
                            try
                            {
                                double.TryParse(minutosConf.Valor, out minutosValue);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        var fechaDesde = DateTime.Now.AddMinutes(minutosValue * -1).AddSeconds(-30);
                        var TipoPago = int.Parse(pago.CodigoTipoPago);
                        existeAnticipo = context.AnticiposxCliente.Where(x => x.CodigoCliente == anticipoPost.CodigoCliente
                                                                            && (x.Fecha >= fechaDesde && x.Fecha <= DateTime.Now)
                                                                            && x.IdTipoPago == TipoPago
                                                                            && x.SpecPago == pago.TipoPagoDetalle
                                                                            && x.Valor == valorPago).Count();

                        if(existeAnticipo == 0)
                        {
                            var anticipo = new AnticiposxCliente
                            {
                                CodigoCliente = anticipoPost.CodigoCliente,
                                Fecha = anticipoPost.Fecha,
                                IdTipoPago = int.Parse(pago.CodigoTipoPago),
                                Referencia = pago.Referencia,
                                FechaCheque = anticipoPost.FechaPago,
                                IdBanco = codigobanco,
                                Sincronizado = false,
                                Valor = valorPago,
                                IdMoneda = pago.IdMoneda,
                                CodigoAsesor = user.UserAccount,
                                Tipo = anticipoPost.Tipo,
                                NumeroRecibo = anticipoPost.NumeroRecibo,
                                NumPedido = anticipoPost.NumPedido,
                                Latitude = (anticipoPost.location != null) ? anticipoPost.location.latitude : null,
                                Longitude = (anticipoPost.location != null) ? anticipoPost.location.longitude : null,
                                SpecPago = pago.TipoPagoDetalle,
                                EsContado = anticipoPost.EsContado == "1" ? true : false,
                                Descuento = 0
                            };
                            context.AnticiposxCliente.Add(anticipo);
                        }
                        else
                        {
                           var reciboXClienteFlotante = new RecibosxClienteFlotante
                            {
                                NumeroRecibo = anticipoPost.NumeroRecibo,
                                CodigoCliente = anticipoPost.CodigoCliente,
                                Fecha = anticipoPost.Fecha,
                                IdTipoPago = int.Parse(pago.CodigoTipoPago),
                                Referencia = pago.Referencia,
                                FechaCheque = anticipoPost.FechaPago,
                                IdBanco = codigobanco,
                                Valor = valorPago,
                                IdMoneda = pago.IdMoneda,
                                Sincronizado = false,
                                CodigoAsesor = asesor.CodigoAsesor,
                                IdFactura = null,
                                Descuento = 0,
                                Latitude = (anticipoPost.location != null) ? anticipoPost.location.latitude : null,
                                Longitude = (anticipoPost.location != null) ? anticipoPost.location.longitude : null,
                                SpecPago = pago.TipoPagoDetalle,
                                EsContado = anticipoPost.EsContado == "1" ? true : false,
                                NumPedido = anticipoPost.NumPedido,
                                Tipo = anticipoPost.Tipo,
                                UsuarioCreacion = user.UserAccount,
                                FechaCreacion = DateTime.Now,
                                Estado = 0  ///0: Pendiente, 1: Sincronizado, 2:Cancelado
                            };
                            context.RecibosxClienteFlotante.Add(reciboXClienteFlotante);
                        }
                       
                        var pagoBD = context.TiposdePago.FirstOrDefault(pa => pa.IdTipoPago.ToString() == pago.CodigoTipoPago);
                        var pagoDetalleBD = context.TiposdePagoDetalle.FirstOrDefault(pd => pd.IdTipoPago.ToString() == pago.CodigoTipoPago && pd.CodigoDetalle == pago.TipoPagoDetalle);
                        var respuestapago = new RespuestaPago
                        {
                            TipoPago = pagoBD.Descripcion,
                            EspecificacionPago = pagoDetalleBD.Descripcion,
                            Fecha = anticipoPost.Fecha,
                            Referencia = pago.Referencia,
                            Monto = pago.Valor,
                        };
                        var bank = context.Bancos.FirstOrDefault(ban => ban.IdBanco.ToString() == pago.IdBanco);
                        if (bank != null)
                        {
                            respuestapago.Banco = bank.Descripcion;
                        }
                        respuestaPagoRecibo.Pagos.Add(respuestapago);
                       
                        asesor.CorrelativoRecibos = numeroCorrelativoRecibo + 1;

                        RespuestaFactura pagoAplicado = new RespuestaFactura
                        {
                            IdFactura = "Anticipo",
                            Fecha = anticipoPost.FechaPago,
                            Dias = 0,
                            TipoDocumento = anticipoPost.Tipo,
                            Aplicado = pago.Valor,
                            Parcial = pago.Valor,
                            Parcial2 = 0,
                        };
                        respuestaPagoRecibo.Total = pago.Valor;
                        respuestaPagoRecibo.CodigoUltimoRecibo = anticipoPost.NumeroRecibo;
                        respuestaPagoRecibo.Facturas.Add(pagoAplicado);

                        ReciboApiModel anticipoAX = new ReciboApiModel
                        {
                            COMPANY = asesor.EmpresaId,
                            ASESOR = asesor.Usuario,
                            ASESOR_NOMBRE = asesor.Nombre,
                            ASESOR_DIARIO = asesor.CodigoAsesor,
                            RECIBO = anticipoPost.NumeroRecibo,
                            CLIENTE = anticipoPost.CodigoCliente,
                            MONEDA = pago.IdMoneda,
                            FECHA = DateTime.Now.ToString("dd/MM/yyyy"),
                            DESCRIPCION = anticipoPost.Descripcion,
                            TOTAL_RECIBO = valorPago.ToString(),
                            TOTAL_FACTURAS = valorPago.ToString(),
                            TOTAL_APLICADO = valorPago.ToString(),
                            TIPO_PAGO = pagoBD.Codigo,
                            SPEC_PAGO = pago.TipoPagoDetalle,
                            BANCO = pago.IdBanco,
                            REFERENCIA = pago.Referencia,
                            FECHA_PAGO = anticipoPost.FechaPago.ToString("dd/MM/yyyy"),
                            FACTURA = pagoAplicado.IdFactura,
                            APLICADO = valorPago.ToString(),
                            DESCUENTO = "0",
                            REF_TRANSOPEN = pago.ReferenciaTransaccionAbierta,
                            ES_CONTADO = anticipoPost.EsContado,
                            NUM_PEDIDO = anticipoPost.NumPedido
                        };
                        recibos.Add(anticipoAX);
                    }

                    _ = PostAnticipoAx(recibos);

                    context.SaveChanges();
                    return Ok(respuestaPagoRecibo);
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }    
        }

        [HttpPost]
        public IHttpActionResult PostRecibo(ReciboPostViewModel reciboPost)
        {
            try
            {
                try
                {
                    var json = new JavaScriptSerializer().Serialize(reciboPost);

                    EscribirEnArchivo($"Recibo At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }

                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
                List<RecibosxClienteViewModel> recibosxCliente = new List<RecibosxClienteViewModel>();
                List<RecibosxClienteFlotanteViewModel> recibosxClienteFlotante = new List<RecibosxClienteFlotanteViewModel>();
                reciboPost.FechaPago = new DateTime(reciboPost.FechaPago.Year, reciboPost.FechaPago.Month, reciboPost.FechaPago.Day);
                var existeRecibo = 0;
                var asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                var PagosBD = context.TiposdePago.AsNoTracking().ToList();
                var BancosBD = context.Bancos.AsNoTracking().ToList();
                var codigoCliente = "";
                int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                string inicialesAsesor = asesor.InicialesNombre;
                var subFacturas = context.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => reciboPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderBy(subFac => subFac.FechaVencimiento).ToList();
                List <ReciboApiModel> recibos = new List<ReciboApiModel>();
                var isOnline = EnLinea(asesor.EmpresaId, asesor.CodigoAsesor);
                foreach (PagosReciboPostViewModel pago in reciboPost.Pagos.OrderBy(pag => pag.Orden))
                {
                    var pagoBD = PagosBD.FirstOrDefault(pa => pa.IdTipoPago.ToString() == pago.CodigoTipoPago);
                    var pagoDetalleBD = context.TiposdePagoDetalle.FirstOrDefault(pd => pd.IdTipoPago.ToString() == pago.CodigoTipoPago && pd.CodigoDetalle == pago.TipoPagoDetalle);
                    var respuestapago = new RespuestaPago
                    {
                        TipoPago = pagoBD.Descripcion,
                        EspecificacionPago = pagoDetalleBD.Descripcion,
                        Fecha = reciboPost.Fecha,
                        Referencia = pago.Referencia,
                        Monto = pago.Valor,
                    };
                    var bank = BancosBD.FirstOrDefault(ban => ban.IdBanco.ToString() == pago.IdBanco);
                    if (bank != null)
                    {
                        respuestapago.Banco = bank.Descripcion;
                    }
                    respuestaPagoRecibo.Pagos.Add(respuestapago);
                    List<ReciboApiModel> recibosXPago = new List<ReciboApiModel>();
                    double valor = pago.Valor;
                    foreach (SubFacturasxCliente subfactura in subFacturas)
                    {
                        double montoAplicado = 0;
                        double valorCuota = Decimal.ToDouble(subfactura.Saldo ?? 0);
                        double valorCuotaOriginal = Decimal.ToDouble(subfactura.Saldo ?? 0);
                        var Factura = context.FacturasxCliente.Where(fa => fa.Factura == subfactura.Factura).FirstOrDefault();
                        Factura.PendienteFactura = Decimal.Parse((valor).ToString());
                        if ((valor > 0) && (valorCuota > 0))
                        {
                            bool aplicaDescuento = false;
                            aplicaDescuento = ((subfactura.Descuento ?? 0) > 0 &&
                                ((subfactura.FechaMaxDescuento.HasValue && reciboPost.FechaPago.Date <= subfactura.FechaMaxDescuento.Value.Date) ||
                                (subfactura.FechaVencimientoDescuento.HasValue && reciboPost.FechaPago.Date <= subfactura.FechaVencimientoDescuento.Value.Date)
                                //reciboPost.FechaPago.Date <= subfactura.FacturasxCliente.FechaMaxDescuento
                                ));
                            if (aplicaDescuento)
                            {
                                valorCuota = Decimal.ToDouble(subfactura.Saldo.Value - subfactura.Descuento.Value);
                            }
                            var pagoValor = Decimal.Parse((valor).ToString());
                            var minutosConf = context.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotanteRecibo");
                            double minutosValue = 1;

                            if (minutosConf != null)
                            {
                                try
                                {
                                    double.TryParse(minutosConf.Valor, out minutosValue);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            var fechaDesde = DateTime.Now.AddMinutes(minutosValue * -1).AddSeconds(-30);
                            var recibo = recibosXPago.FirstOrDefault(rec => rec.TIPO_PAGO == pago.CodigoTipoPago && rec.REFERENCIA == pago.Referencia && rec.FACTURA == subfactura.Factura);
                            RecibosxClienteViewModel reciboXCliente = recibosxCliente.FirstOrDefault(recXCli => recXCli.IdTipoPago.ToString() == pago.CodigoTipoPago && recXCli.Referencia == pago.Referencia);
                            RecibosxClienteFlotanteViewModel reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel();
                            existeRecibo = context.RecibosxCliente.Where(x => x.CodigoCliente == subfactura.CodigoCliente
                                                                            && (x.Fecha >= fechaDesde && x.Fecha <= DateTime.Now)
                                                                            && x.IdTipoPago == pagoBD.IdTipoPago
                                                                            && x.Referencia == pago.Referencia
                                                                            && x.SpecPago == pago.TipoPagoDetalle
                                                                            && x.Valor == pagoValor
                                                                            && x.IdFactura == subfactura.IdFactura).Count();
                            if (recibo == null)
                            {
                                recibo = new ReciboApiModel
                                {
                                    COMPANY = subfactura.EmpresaId,
                                    ASESOR = asesor.Usuario,
                                    ASESOR_NOMBRE = asesor.Nombre,
                                    ASESOR_DIARIO = asesor.CodigoAsesor,
                                    RECIBO =  reciboPost.NumeroRecibo,
                                    CLIENTE = subfactura.CodigoCliente,
                                    MONEDA = pago.IdMoneda,
                                    FECHA = DateTime.Now.ToString("dd/MM/yyyy"),
                                    DESCRIPCION = reciboPost.Descripcion,
                                    TIPO_PAGO = pagoBD.Codigo,
                                    SPEC_PAGO = pago.TipoPagoDetalle,
                                    BANCO = bank != null ? bank.NombreBanco : "",
                                    REFERENCIA = pago.Referencia,
                                    FECHA_PAGO = reciboPost.FechaPago.ToString("dd/MM/yyyy"),
                                    FACTURA = subfactura.Factura,
                                    APLICADO = "0",
                                    DESCUENTO = "0",
                                    REF_TRANSOPEN = subfactura.Referencia,
                                };
                                recibosXPago.Add(recibo);
                                //if (reciboPost.Pagos.Count() > 1)
                                    //numeroCorrelativoRecibo++;
                            }
                            if (reciboXCliente == null && existeRecibo == 0)
                            {
                                codigoCliente = recibo.CLIENTE;
                                reciboXCliente = new RecibosxClienteViewModel
                                {
                                    NumeroRecibo = recibo.RECIBO,
                                    CodigoCliente = recibo.CLIENTE,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = pagoBD.IdTipoPago,
                                    Referencia = recibo.REFERENCIA,
                                    FechaPago = reciboPost.FechaPago,
                                    IdBanco = bank?.IdBanco,
                                    Valor = 0,
                                    IdMoneda = pago.IdMoneda,
                                    Sincronizado = false,
                                    CodigoAsesor = asesor.CodigoAsesor,
                                    IdFactura = subfactura.IdFactura,
                                    Descuento = 0,
                                    Latitude = (reciboPost.location != null) ? reciboPost.location.latitude : null,
                                    Longitude = (reciboPost.location != null) ? reciboPost.location.longitude : null,
                                    SpecPago = pago.TipoPagoDetalle
                                };
                                recibosxCliente.Add(reciboXCliente);
                            }
                            else
                            {
                                codigoCliente = recibo.CLIENTE;
                                reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel
                                {
                                    NumeroRecibo = recibo.RECIBO,
                                    CodigoCliente = recibo.CLIENTE,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = pagoBD.IdTipoPago,
                                    Referencia = recibo.REFERENCIA,
                                    FechaPago = reciboPost.FechaPago,
                                    IdBanco = bank?.IdBanco,
                                    Valor = 0,
                                    IdMoneda = pago.IdMoneda,
                                    Sincronizado = false,
                                    CodigoAsesor = asesor.CodigoAsesor,
                                    IdFactura = subfactura.IdFactura,
                                    Descuento = 0,
                                    Latitude = (reciboPost.location != null) ? reciboPost.location.latitude : null,
                                    Longitude = (reciboPost.location != null) ? reciboPost.location.longitude : null,
                                    SpecPago = pago.TipoPagoDetalle,
                                    Estado = 0  ///0: Pendiente, 1: Sincronizado, 2:Cancelado
                                };
                                recibosxClienteFlotante.Add(reciboXClienteFlotante);
                            }
                            double aplicadoDouble = 0;
                            double.TryParse(recibo.APLICADO, out aplicadoDouble);
                            RecibosDetalleViewModel detalleReciboXCliente = new RecibosDetalleViewModel
                            {
                                IdSubFactura = subfactura.IdSubFactura,
                                Descuento = 0
                            };
                            if (Math.Round(valorCuota, 2) > Math.Round(valor, 2))
                            {
                                detalleReciboXCliente.Valor = Decimal.Parse((valor).ToString());
                                recibo.APLICADO = (aplicadoDouble + valor).ToString();
                                subfactura.Saldo = (subfactura.Saldo ?? 0) - detalleReciboXCliente.Valor;
                                detalleReciboXCliente.EsAbono = true;
                                montoAplicado = valor;
                                valor = 0;
                            }
                            else
                            {
                                detalleReciboXCliente.Valor = Decimal.Parse((valorCuota).ToString());

                                recibo.APLICADO = detalleReciboXCliente.Valor.ToString();
                                valor -= valorCuota;
                                montoAplicado = valorCuota;
                                if (aplicaDescuento)
                                {
                                    detalleReciboXCliente.Descuento = subfactura.Descuento;
                                    recibo.DESCUENTO = (decimal.Parse(recibo.DESCUENTO) + subfactura.Descuento).ToString();
                                }
                                subfactura.Saldo = 0;
                                detalleReciboXCliente.EsAbono = false;
                            }
                            context.SaveChanges();

                            if(existeRecibo == 0)
                            {
                                reciboXCliente.Descuento += detalleReciboXCliente.Descuento;
                                reciboXCliente.Valor += detalleReciboXCliente.Valor;
                                reciboXCliente.DetalleRecibo.Add(detalleReciboXCliente);
                            }
                            else
                            {
                                reciboXClienteFlotante.Descuento += detalleReciboXCliente.Descuento;
                                reciboXClienteFlotante.Valor += detalleReciboXCliente.Valor;
                                reciboXClienteFlotante.DetalleRecibo.Add(detalleReciboXCliente);
                            }


                            var pagoAplicado = respuestaPagoRecibo.Facturas.FirstOrDefault(fact => fact.IdFactura == recibo.FACTURA);
                            var fechaFactura = context.FacturasxCliente.FirstOrDefault(x=>x.Factura == recibo.FACTURA).FechaFactura;
                            if (pagoAplicado == null)
                            {
                                TimeSpan ts = reciboPost.Fecha - subfactura.FechaVencimiento.Value;

                                int dias = ts.Days;

                                pagoAplicado = new RespuestaFactura
                                {
                                    IdFactura = recibo.FACTURA,
                                    NumeroFEL = subfactura.NumeroFEL,
                                    Fecha = fechaFactura.Value,
                                    Dias = dias,
                                    TipoDocumento = subfactura.FacturasxCliente.Tipo,
                                    EsAbono = detalleReciboXCliente.EsAbono,
                                };
                                respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                            }
                            respuestaPagoRecibo.Total += montoAplicado;
                            pagoAplicado.Aplicado += montoAplicado;
                            pagoAplicado.Parcial += valorCuotaOriginal;

                            respuestaPagoRecibo.CodigoUltimoRecibo = recibo.RECIBO;
                            try
                            {
                                pagoAplicado.Parcial2 += Double.Parse(recibo.DESCUENTO);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    Double totalRecibo = 0;
                    recibosXPago.ForEach(recXPag => totalRecibo += Double.Parse(recXPag.APLICADO));
                    int totalFacturas = recibosXPago.GroupBy(recXPag => recXPag.FACTURA).Count();
                    recibosXPago.ForEach(recXPag =>
                    {
                        recXPag.TOTAL_APLICADO = totalRecibo.ToString();
                        recXPag.TOTAL_RECIBO = totalRecibo.ToString();
                        recXPag.TOTAL_FACTURAS = totalFacturas.ToString();
                    });
                    recibos.AddRange(recibosXPago);
                }
                if (reciboPost.SaldoFavor > 0)
                {
                    RespuestaFactura pagoAplicado = new RespuestaFactura
                    {
                        IdFactura = "SALDO_FAVOR",
                        Fecha = DateTime.Today,
                        Dias = 0,
                        TipoDocumento = "Pago",
                        Aplicado = reciboPost.SaldoFavor,
                        Parcial = reciboPost.SaldoFavor,
                    };
                    respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                    respuestaPagoRecibo.Total += reciboPost.SaldoFavor;
                    if(existeRecibo == 0)
                    {
                        var primerRecibo = recibosxCliente.FirstOrDefault();
                        primerRecibo.Valor += decimal.Parse(reciboPost.SaldoFavor.ToString());
                        primerRecibo.DetalleRecibo.Add(new RecibosDetalleViewModel
                        {
                            Valor = decimal.Parse(reciboPost.SaldoFavor.ToString()),
                            ValorSinDescuento = decimal.Parse(reciboPost.SaldoFavor.ToString())
                        });
                    }
                    else
                    {
                        var primerRecibo = recibosxClienteFlotante.FirstOrDefault();
                        primerRecibo.Valor += decimal.Parse(reciboPost.SaldoFavor.ToString());
                        primerRecibo.DetalleRecibo.Add(new RecibosDetalleViewModel
                        {
                            Valor = decimal.Parse(reciboPost.SaldoFavor.ToString()),
                            ValorSinDescuento = decimal.Parse(reciboPost.SaldoFavor.ToString())
                        });
                    }
                   
                }
               
                if (isOnline && existeRecibo == 0)
                {
                    try
                    {
                        var reciboHeaders = new List<ReciboApiModel>();
                        var client = new RestClient();
                        var request = new RestRequest($"{Enviroment.CRMWebServiceURLApi}recibos/upload", Method.POST)
                        {
                            RequestFormat = DataFormat.Json
                        };
                        request.AddHeader("Content-type", "application/json; charset=utf-8");
                        request.Parameters.Clear();
                        request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(recibos), ParameterType.RequestBody);
                        var respuesta = client.Execute(request);

                        if (respuesta.IsSuccessful && respuesta.Content.Equals("\"\""))
                        {
                            using (AVentasEntities context = new AVentasEntities())
                            {
                                asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                                if (reciboPost.Pagos.Count() == 1)
                                {
                                    numeroCorrelativoRecibo++;
                                    asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                                    context.SaveChanges();
                                }
                            }
                            AsyncSqlInsert.IngresarRecibos(recibosxCliente,true);

                            foreach (var iter in recibos)
                            {
                                using (AVentasEntities ctx = new AVentasEntities())
                                {
                                    var factura = ctx.FacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA && x.EmpresaId == iter.COMPANY);

                                    if (factura != null)
                                    {
                                        factura.Saldo -= iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                                    }

                                    var subfactura = ctx.SubFacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA & x.EmpresaId == iter.COMPANY & x.Referencia == iter.REF_TRANSOPEN);

                                    if (subfactura != null)
                                    {
                                        subfactura.Saldo -= iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                                    }

                                    ctx.SaveChanges();
                                }
                            }

                            syncCuentaCorriente.SyncFacturas(asesor.EmpresaId, codigoCliente);
                            syncCuentaCorriente.SyncSubFacturas(asesor.EmpresaId, codigoCliente, asesor.CodigoAsesor);

                            return Ok(respuestaPagoRecibo);
                        }
                        else
                        {
                            return BadRequest(respuesta.Content);

                        }
                    }
                    catch (Exception)
                    {
                        return BadRequest(Newtonsoft.Json.JsonConvert.SerializeObject(recibos));

                    }
                }
                else
                {
                    if(existeRecibo > 0)
                    {
                        AsyncSqlInsert.IngresarRecibosFlotante(recibosxClienteFlotante);
                        return Ok(respuestaPagoRecibo);
                    }

                    AsyncSqlInsert.IngresarRecibos(recibosxCliente, false);
                    foreach (var iter in recibos)
                    {
                        using (AVentasEntities ctx = new AVentasEntities())
                        {
                            asesor = ctx.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                            if (reciboPost.Pagos.Count() == 1)
                            {
                                numeroCorrelativoRecibo++;
                                asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                                ctx.SaveChanges();
                            }
                            var factura = ctx.FacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA && x.EmpresaId == iter.COMPANY);

                            if (factura != null)
                            {
                                factura.Saldo -= iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                            }

                            var subfactura = ctx.SubFacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA & x.EmpresaId == iter.COMPANY & x.Referencia == iter.REF_TRANSOPEN);

                            if (subfactura != null)
                            {
                                subfactura.Saldo -= iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                            }

                            ctx.SaveChanges();
                        }
                    }
                    return Ok(respuestaPagoRecibo);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("api/Recibo/PostReciboAx")]
        [HttpPost]
        public async Task<IHttpActionResult> PostReciboAx(List<ReciboApiModel> recibos)
        {
            if (EnLinea(recibos[0].COMPANY, recibos[0].ASESOR))
            {
                try
                {
                    var client = new RestClient();
                    var request = new RestRequest($"{Enviroment.CRMWebServiceURLApi}recibos/upload", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader("Content-type", "application/json; charset=utf-8");
                    request.Parameters.Clear();
                    request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(recibos), ParameterType.RequestBody);
                    var respuesta = client.Execute(request);

                    if (respuesta.IsSuccessful && respuesta.Content.Equals("\"\""))
                    {
                        using(var ctx = new AVentasEntities())
                        {
                            var numeroRecibo = recibos[0].RECIBO;
                                var recibo = await ctx.RecibosxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo == numeroRecibo);
                                recibo.Sincronizado = true;
                            await ctx.SaveChangesAsync();
                        }

                        syncCuentaCorriente.SyncFacturas(recibos[0].COMPANY, recibos[0].CLIENTE);
                        syncCuentaCorriente.SyncSubFacturas(recibos[0].COMPANY, recibos[0].CLIENTE, recibos[0].ASESOR);
                        return Ok($"El recibo {recibos[0].RECIBO} ha sido sincronizado exitosamente con AX.");
                    }
                    else
                    {
                        return BadRequest(respuesta.Content);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(Newtonsoft.Json.JsonConvert.SerializeObject(recibos));
                }
            }
            else
            {
                return BadRequest("El servidor de AX no esta disponible.");
            }
        }

        [Route("api/Recibo/PostAnticipoAx")]
        [HttpPost]
        public async Task<IHttpActionResult> PostAnticipoAx(List<ReciboApiModel> recibos)
        {
            if (EnLinea(recibos[0].COMPANY, recibos[0].ASESOR))
            {
                try
                {
                    var client = new RestClient();
                    var request = new RestRequest($"{Enviroment.CRMWebServiceURLApi}recibos/upload", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader("Content-type", "application/json; charset=utf-8");
                    request.Parameters.Clear();
                    request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(recibos), ParameterType.RequestBody);
                    var respuesta = client.Execute(request);

                    if (respuesta.IsSuccessful && respuesta.Content.Equals("\"\""))
                    {
                        using (var ctx = new AVentasEntities())
                        {
                            var numeroRecibo = recibos[0].RECIBO;
                            var recibo = await ctx.AnticiposxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo == numeroRecibo);
                            recibo.Sincronizado = true;
                            await ctx.SaveChangesAsync();
                        }

                        syncCuentaCorriente.SyncFacturas(recibos[0].COMPANY, recibos[0].CLIENTE);
                        syncCuentaCorriente.SyncSubFacturas(recibos[0].COMPANY, recibos[0].CLIENTE, recibos[0].ASESOR);
                        return Ok($"El recibo {recibos[0].RECIBO} ha sido sincronizado exitosamente con AX.");
                    }
                    else
                    {
                        return BadRequest(respuesta.Content);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(Newtonsoft.Json.JsonConvert.SerializeObject(recibos));
                }
            }
            else
            {
                return BadRequest("El servidor de AX no esta disponible.");
            }
        }

        [HttpGet]
        [Route("~/api/Recibo/flotante/{FechaInicio}/{FechaFin}/{estado}")]
        public IHttpActionResult GetFlotantes(DateTime FechaInicio, DateTime FechaFin, int estado)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }

                    var Recibos = ctx.RecibosxClienteFlotante.Where(r => r.Estado == estado && r.Fecha >= FechaInicio && r.Fecha < FechaFin).Select(rec => new RecibosxClienteViewModel
                    {
                        Id = rec.ReciboId,
                        Anticipo = false,
                        Estado = rec.Estado,
                        NombreAsesor = ctx.Asesores.FirstOrDefault(x => x.CodigoAsesor == rec.CodigoAsesor).Nombre,
                        Asesor = rec.CodigoAsesor,
                        NumeroRecibo = rec.NumeroRecibo,
                        CodigoCliente = rec.CodigoCliente,
                        Fecha = rec.Fecha,
                        IdTipoPago = rec.IdTipoPago,
                        Referencia = rec.Referencia,
                        FechaPago = rec.FechaCheque,
                        IdBanco = rec.IdBanco,
                        Valor = rec.Valor,
                        IdMoneda = ctx.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == rec.IdMoneda).Moneda,
                        Sincronizado = rec.Sincronizado,
                        CodigoAsesor = rec.CodigoAsesor,
                        IdFactura = rec.IdFactura,
                        Longitude = rec.Longitude,
                        Latitude = rec.Latitude,
                        DescripcionBanco = ctx.Bancos.Where(banco => banco.IdBanco == rec.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                        Descuento = rec.Descuento,
                        Cliente = ctx.Clientes.Where(cli => cli.CodigoCliente == rec.CodigoCliente).Select(cli => new ClienteViewModel
                        {
                            Codigo = cli.CodigoCliente,
                            Nombre = cli.Nombre,
                            Direccion = cli.Direccion,
                            Moneda = cli.IdMoneda
                        }).FirstOrDefault(),
                        TipoPago = ctx.TiposdePago.Where(tp => tp.IdTipoPago == rec.IdTipoPago).Select(tp => new TipoPagoViewModel
                        {
                            IdTipoPago = tp.IdTipoPago,
                            Codigo = tp.Codigo,
                            Descripcion = tp.Descripcion,
                            Tipo = tp.Tipo,
                            EmpresaId = tp.EmpresaId,
                            TiposdePagoDetalle = tp.TiposdePagoDetalle.Where(d => d.CodigoDetalle == rec.SpecPago).Select(pd => new TipoPagoDetalleViewModel
                            {
                                Codigo = pd.Codigo,
                                CodigoDetalle = pd.CodigoDetalle,
                                Descripcion = pd.Descripcion
                            }).ToList(),
                        }).FirstOrDefault(),
                        DetalleRecibo = rec.RecibosDetalleFlotante.Select(recDet =>
                        recDet.SubFacturasxCliente != null ?
                        new RecibosDetalleViewModel
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            Factura = recDet.SubFacturasxCliente.Factura,
                            NumeroFel = recDet.SubFacturasxCliente.NumeroFEL,
                            FechaFactura = recDet.SubFacturasxCliente.FacturasxCliente.FechaFactura,
                            Tipo = rec.FacturasxCliente.Tipo,
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = recDet.IdSubFactura,
                            Valor = recDet.Valor,
                            ValorFactura = recDet.SubFacturasxCliente.FacturasxCliente.TotalFactura,
                            ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                            Descuento = recDet.Descuento,
                            EsAbono = recDet.EsAbono,
                            DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                        } : new RecibosDetalleViewModel
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            Factura = "SALDO_FAVOR",
                            NumeroFel = "",
                            FechaFactura = null,
                            Tipo = "Pago",
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = null,
                            Valor = recDet.Valor,
                            ValorFactura = 0,
                            ValorSinDescuento = recDet.Valor,
                            Descuento = 0,
                            EsAbono = true,
                            DiasVencimiento = 0,
                        }
                        ).ToList()
                    }).ToList();

                    return Ok(Recibos);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Recibo/flotante/cancelar/{id}")]
        public async Task<IHttpActionResult> CancelarFlotante(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var recibo = await ctx.RecibosxClienteFlotante.FirstOrDefaultAsync(x => x.ReciboId == id);

                    if (recibo == null)
                    {
                        return BadRequest("El recibo no existe.");
                    }

                    recibo.UsuarioModificacion = user.UserAccount;
                    recibo.FechaModificacion = DateTime.Now;
                    recibo.Estado = 2;
                    int res = await ctx.SaveChangesAsync();
                    return Ok(res);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Recibo/flotante/sincronizar/{id}")]
        public async Task<IHttpActionResult> SincronizarFlotante(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var recibo = await ctx.RecibosxClienteFlotante.FirstOrDefaultAsync(x => x.ReciboId == id);

                    if (recibo == null)
                    {
                        return BadRequest("El recibo no existe.");
                    }

                    var asesor = await ctx.Asesores.FirstOrDefaultAsync(ase => ase.Usuario == recibo.CodigoAsesor);
                    int numeroCorelativo = asesor.CorrelativoRecibos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";

                    var reciboBD = new RecibosxCliente()
                    {
                        NumeroRecibo = numeroReferencia,
                        CodigoCliente = recibo.CodigoCliente,
                        Fecha = recibo.Fecha,
                        IdTipoPago = recibo.IdTipoPago,
                        SpecPago = recibo.SpecPago,
                        Referencia = recibo.Referencia,
                        FechaCheque = recibo.FechaCheque,
                        IdBanco = recibo.IdBanco,
                        IdCuentaBancaria = recibo.IdCuentaBancaria,
                        Valor = recibo.Valor,
                        IdMoneda = recibo.IdMoneda,
                        CodigoAsesor = recibo.CodigoAsesor,
                        FechaCreacion = recibo.FechaCreacion,
                        UsuarioCreacion = recibo.UsuarioCreacion,
                        FechaModificacion = recibo.FechaModificacion,
                        UsuarioModificacion = recibo.UsuarioModificacion,
                        IdFactura = recibo.IdFactura,
                        Descuento = recibo.Descuento,
                        Latitude = recibo.Latitude,
                        Longitude = recibo.Longitude,
                        RecibosDetalle = recibo.RecibosDetalleFlotante.Select(d => new RecibosDetalle()
                        {
                            IdReciboDetalle = d.IdReciboDetalle,
                            ReciboId = d.ReciboId,
                            IdSubFactura = d.IdSubFactura,
                            Valor = d.Valor,
                            Descuento = d.Descuento,
                            EsAbono = d.EsAbono,
                        }).ToList()
                    };

                    ctx.RecibosxCliente.Add(reciboBD);
                    int resultado = await ctx.SaveChangesAsync();

                    if (resultado > 0)
                    {
                        recibo.ReciboIdGenerado = numeroReferencia;
                        recibo.UsuarioModificacion = user.UserAccount;
                        recibo.Estado = 1;
                        recibo.FechaModificacion = DateTime.Now;
                        asesor.CorrelativoRecibos = asesor.CorrelativoRecibos + 1;
                        await ctx.SaveChangesAsync();
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
        public void EscribirEnArchivo(string Message)
        {
            try
            {
                #region Creacion Carpeta
                string path = @"C:\AVentasAPIRecibos";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                #endregion Creacion Carpeta

                #region Creacion Archivo
                string filepath = path + "\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
                #endregion Creacion Archivo
            }
            catch (Exception ex)
            {

            }
        }

    }
    public class RespuestaRecibo
    {
        public DateTime Fecha { get; set; }
        public double Total { get; set; }
        public string CodigoUltimoRecibo { get; set; }
        public List<RespuestaPago> Pagos { get; set; }
        public List<RespuestaFactura> Facturas { get; set; }
        public RespuestaRecibo()
        {
            Fecha = DateTime.Now;
            Pagos = new List<RespuestaPago>();
            Facturas = new List<RespuestaFactura>();
        }
    }
    public class RespuestaFactura
    {
        public string IdFactura { get; set; }
        public string NumeroFEL { get; set; }
        public DateTime Fecha { get; set; }
        public double Parcial { get; set; }
        public double Aplicado { get; set; }
        public double Parcial2 { get; set; }
        public int Dias { get; set; }
        public string TipoDocumento { get; set; }
        public bool? EsAbono { get; set; }
    }
    public class RespuestaPago
    {
        public string TipoPago { get; set; }
        public string EspecificacionPago { get; set; }
        public DateTime Fecha { get; set; }
        public string Banco { get; set; }
        public string Referencia { get; set; }
        public double Monto { get; set; }
    }
}
