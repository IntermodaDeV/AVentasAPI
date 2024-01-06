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
        [Route("~/api/recibos/firma/{empresa}")]
        public async Task<IHttpActionResult> GetFirmaAsesor(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId == empresa);

                    if (asesor.firma == null)
                    {
                        return Ok("");
                    }

                    string firma = "";
                    firma = "data:image/png;base64," + Convert.ToBase64String(asesor.firma);
                    return Ok(firma);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("~/api/recibos/correlativo/{empresa}")]
        public async Task<IHttpActionResult> GetCorrelativo(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId==empresa);
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


                    var clientes = context.Clientes.Where(x => x.CodigoAsesor.ToUpper() == Asesor.ToUpper()).Select(x => x.CodigoCliente);

                    var Recibos = context.RecibosxCliente.Where(r => clientes.Contains(r.CodigoCliente) && r.Fecha >= FechaInicio && r.Fecha < FechaFin).Select(rec => new RecibosxClienteViewModel
                    {
                        NumeroCopia = context.LogRecibo.Where(x => x.ReciboId == rec.ReciboId).Count() + 1,
                        Anticipo = false,
                        NombreAsesor = "",
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
                        //firmaByte = rec.firma,
                        firma = "",
                        locationCliente = new LocationCliente
                        {
                            latitude = context.Clientes.FirstOrDefault(x => x.CodigoCliente == rec.CodigoCliente).Latitud,
                            longitude = context.Clientes.FirstOrDefault(x => x.CodigoCliente == rec.CodigoCliente).Longitud
                        },
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
                            cuota = recDet.SubFacturasxCliente.NumeroCuota,
                            Factura = recDet.SubFacturasxCliente.Factura,
                            NumeroFel = recDet.SubFacturasxCliente.NumeroFEL,
                            FechaFactura = recDet.SubFacturasxCliente.FacturasxCliente.FechaFactura,
                            Tipo = recDet.SubFacturasxCliente.FacturasxCliente.Tipo,
                            ReciboId = recDet.ReciboId,
                            IdSubFactura = recDet.IdSubFactura,
                            Valor = recDet.Valor,
                            ValorFactura = recDet.ValorFactura ?? recDet.SubFacturasxCliente.FacturasxCliente.TotalFactura,
                            ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                            Descuento = recDet.Descuento,
                            EsAbono = recDet.EsAbono,
                            DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                        } : new RecibosDetalleViewModel
                        {
                            IdReciboDetalle = recDet.IdReciboDetalle,
                            cuota = null,
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

                    var anticiposXAsesor = context.AnticiposxCliente.Where(r => clientes.Contains(r.CodigoCliente) && r.Fecha >= FechaInicio && r.Fecha < FechaFin).Select(ant => new RecibosxClienteViewModel
                    {
                        Anticipo = true,
                        NombreAsesor = "",
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
                        //firmaByte = ant.firma,
                        firma = "",
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
                            TiposdePagoDetalle = tp.TiposdePagoDetalle.Where(d => d.CodigoDetalle == ant.SpecPago).Select(pd => new TipoPagoDetalleViewModel
                            {
                                Codigo = pd.Codigo,
                                CodigoDetalle = pd.CodigoDetalle,
                                Descripcion = pd.Descripcion
                            }).ToList()
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



                    /*foreach (var recibo in ListaRecibos)
                    {
                        if (recibo.firmaByte != null)
                        {
                            string firma = "";
                            firma = "data:image/png;base64," + Convert.ToBase64String(recibo.firmaByte);
                            recibo.firma = firma;
                            recibo.firmaByte = null;
                        }
                        else
                        {
                            recibo.firma = "";
                            recibo.firmaByte = null;
                        }
                    }*/

                    return Ok(ListaRecibos.OrderByDescending(x=>x.FechaCreacion));
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
                    var ReciboDetalle = ctx.RecibosDetalle.Where(s => s.RecibosxCliente.NumeroRecibo == recibo && s.IdSubFactura!=null).ToList();
                    var saldoFavor = ctx.RecibosDetalle.FirstOrDefault(x=>x.ReciboId == Recibo.ReciboId && x.IdSubFactura==null);
                    string empresa = Recibo.CodigoCliente.Substring(0, 4);
                    var asesor = ctx.Asesores.Where(a => a.CodigoAsesor == Recibo.CodigoAsesor && a.EmpresaId==empresa).FirstOrDefault();
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

                        if (Detalle.SubFacturasxCliente.EmpresaId.ToUpper() == "IMGT")
                        {
                            var banco = Banco != null ? Banco.NombreBanco : "";
                            Recibos.DESCRIPCION = $"{Recibo.NumeroRecibo} {Recibo.Referencia} {banco} {Recibo.Fecha.Value.ToString("dd/MM/yyyy")} {Detalle.SubFacturasxCliente.CodigoCliente}";
                        }

                        ReciboSincronizar.Add(Recibos);
                    }

                    if (saldoFavor != null)
                    {
                        string ultimoValor = ReciboSincronizar[ReciboSincronizar.Count() - 1].APLICADO;
                        decimal valorConSaldoFavor = decimal.Parse(ultimoValor) + saldoFavor.Valor.Value;
                        ReciboSincronizar[ReciboSincronizar.Count() - 1].APLICADO = valorConSaldoFavor.ToString();
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
                    string empresa = Recibo.CodigoCliente.Substring(0, 4);
                    var asesor = ctx.Asesores.Where(a => a.CodigoAsesor == Recibo.CodigoAsesor && a.EmpresaId == empresa).FirstOrDefault();
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
                try
                {
                    var json = new JavaScriptSerializer().Serialize(anticipoPost);

                    EscribirEnArchivo($"Recibo At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }
                RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                var asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount && ase.EmpresaId==anticipoPost.EmpresaUsuario);
                var correlativo = anticipoPost.ReciboProforma ? anticipoPost.NumeroRecibo.Substring(2, anticipoPost.NumeroRecibo.Length - 2) : anticipoPost.NumeroRecibo;
                if (anticipoPost.Pagos != null)
                {
                    var existeAnticipo = 0;
                    var existeProforma = 0;
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
                        if (anticipoPost.ReciboProforma)
                        {
                            existeProforma = context.RecibosProforma.Where(x => x.CodigoCliente == anticipoPost.CodigoCliente
                                                                           && (x.Fecha >= fechaDesde && x.Fecha <= DateTime.Now)
                                                                           && x.IdTipoPago == TipoPago
                                                                           && x.SpecPago == pago.TipoPagoDetalle
                                                                           && x.Valor == valorPago).Count();  
                            if (existeProforma == 0)
                            {
                                existeProforma = context.RecibosProforma.Where(x => x.NumeroProforma == anticipoPost.NumeroRecibo).Count();
                            }

                            if (existeProforma == 0)
                            {
                                var proforma = new RecibosProforma
                                {
                                    NumeroProforma=anticipoPost.NumeroRecibo,
                                    CodigoCliente = anticipoPost.CodigoCliente,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = int.Parse(pago.CodigoTipoPago),
                                    Referencia = pago.Referencia,
                                    FechaCheque = anticipoPost.FechaPago,
                                    IdBanco = codigobanco,
                                    Valor = valorPago,
                                    IdMoneda = pago.IdMoneda,
                                    CodigoAsesor = user.UserAccount,
                                    SpecPago = pago.TipoPagoDetalle,
                                    UsuarioCreacion = user.UserAccount,
                                    FechaCreacion = DateTime.Now,
                                    Descuento = 0
                                };
                                context.RecibosProforma.Add(proforma);

                                var anticipo = new AnticiposxCliente
                                {
                                    CodigoCliente = anticipoPost.CodigoCliente,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = int.Parse(pago.CodigoTipoPago),
                                    Referencia = pago.Referencia,
                                    FechaCheque = anticipoPost.FechaPago,
                                    IdBanco = codigobanco,
                                    Sincronizado = false,
                                    Valor = valorPago,
                                    IdMoneda = pago.IdMoneda,
                                    CodigoAsesor = user.UserAccount,
                                    Tipo = anticipoPost.Tipo,
                                    NumeroRecibo = correlativo,
                                    NumPedido = anticipoPost.NumPedido,
                                    Latitude = (anticipoPost.location != null) ? anticipoPost.location.latitude : null,
                                    Longitude = (anticipoPost.location != null) ? anticipoPost.location.longitude : null,
                                    SpecPago = pago.TipoPagoDetalle,
                                    EsContado = anticipoPost.EsContado == "1" ? true : false,
                                    UsuarioCreacion = user.UserAccount,
                                    FechaCreacion = DateTime.Now,
                                    Descuento = 0,
                                    Origen = "Web",
                                    firma = asesor.firma
                                };
                                context.AnticiposxCliente.Add(anticipo);
                            }
                        }
                        else
                        {
                            existeAnticipo = context.AnticiposxCliente.Where(x => x.NumeroRecibo == anticipoPost.NumeroRecibo).Count();
                            
                            if (existeAnticipo == 0)
                            {
                                existeAnticipo = context.RecibosxCliente.Where(x => x.NumeroRecibo == anticipoPost.NumeroRecibo).Count();
                            }

                            if (existeAnticipo == 0)
                            {
                                existeAnticipo = context.AnticiposxCliente.Where(x => x.CodigoCliente == anticipoPost.CodigoCliente
                                                                             && (x.Fecha >= fechaDesde && x.Fecha <= DateTime.Now)
                                                                             && x.IdTipoPago == TipoPago
                                                                             && x.SpecPago == pago.TipoPagoDetalle
                                                                             && x.Valor == valorPago).Count();
                            }
                            if (existeAnticipo == 0)
                            {
                                var anticipo = new AnticiposxCliente
                                {
                                    CodigoCliente = anticipoPost.CodigoCliente,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = int.Parse(pago.CodigoTipoPago),
                                    Referencia = pago.Referencia,
                                    FechaCheque = anticipoPost.FechaPago,
                                    IdBanco = codigobanco,
                                    Sincronizado = false,
                                    Valor = valorPago,
                                    IdMoneda = pago.IdMoneda,
                                    CodigoAsesor = user.UserAccount,
                                    Tipo = anticipoPost.Tipo,
                                    NumeroRecibo = correlativo,
                                    NumPedido = anticipoPost.NumPedido,
                                    Latitude = (anticipoPost.location != null) ? anticipoPost.location.latitude : null,
                                    Longitude = (anticipoPost.location != null) ? anticipoPost.location.longitude : null,
                                    SpecPago = pago.TipoPagoDetalle,
                                    EsContado = anticipoPost.EsContado == "1" ? true : false,
                                    UsuarioCreacion = user.UserAccount,
                                    FechaCreacion = DateTime.Now,
                                    Descuento = 0,
                                    Origen="Web",
                                    //firma = asesor.firma
                                };
                                context.AnticiposxCliente.Add(anticipo);
                            }
                            else
                            {
                                var reciboXClienteFlotante = new RecibosxClienteFlotante
                                {
                                    NumeroRecibo = correlativo,
                                    CodigoCliente = anticipoPost.CodigoCliente,
                                    Fecha = DateTime.Now,
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


                        if (existeAnticipo > 0)
                        {
                            respuestaPagoRecibo.Mensaje = "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de recibos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento de créditos para que procedan a revisar y gestionar su recibo para que sea válido.";
                            return Ok(respuestaPagoRecibo);
                        }
                       
                            ReciboApiModel anticipoAX = new ReciboApiModel
                        {
                            COMPANY = asesor.EmpresaId,
                            ASESOR = asesor.Usuario,
                            ASESOR_NOMBRE = asesor.Nombre,
                            ASESOR_DIARIO = asesor.CodigoAsesor,
                            RECIBO = correlativo,
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

                        if (asesor.EmpresaId.ToUpper() == "IMGT")
                        {
                            var banco = bank != null ? bank.NombreBanco : "";
                            anticipoAX.DESCRIPCION = $"{anticipoPost.NumeroRecibo} {pago.Referencia} {banco} {anticipoAX.FECHA} {anticipoPost.CodigoCliente}";
                        }

                        recibos.Add(anticipoAX);
                    }
                    int affectedRows = context.SaveChanges();

                    if(affectedRows> 0)
                    {
                        AsyncSqlInsert.ValidarCorrelativoRecibo(asesor.CodigoAsesor, asesor.EmpresaId);
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
                                        var recibo =  ctx.AnticiposxCliente.FirstOrDefault(x => x.NumeroRecibo == numeroRecibo);
                                        recibo.Sincronizado = true;
                                        ctx.SaveChanges();
                                    }

                                    syncCuentaCorriente.SyncFacturas(recibos[0].COMPANY, recibos[0].CLIENTE, recibos[0].ASESOR);
                                    syncCuentaCorriente.SyncSubFacturas(recibos[0].COMPANY, recibos[0].CLIENTE, recibos[0].ASESOR);
                                }
                                
                            }
                            catch (Exception)
                            {
                                
                            }
                        }
                        respuestaPagoRecibo.Mensaje = "";
                        return Ok(respuestaPagoRecibo);
                    }
                    else
                    {
                        return BadRequest("Error al guardar el recibo");
                    }
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }    
        }

        private bool noEsExponencial(double valor)
        {
            var numeroString = valor.ToString();
            return !(numeroString.Contains('E') || numeroString.Contains('e'));
        }

        private decimal TotalDocumentosAplicados(string factura, string codigoCliente)
        {
            var valor = 0m;
            using(var ctx = new AVentasEntities())
            {
                IQueryable<DocumentosAplicadosAFacturas> documentos = ctx.DocumentosAplicadosAFacturas.Where(x => x.Factura == factura && x.CodigoCliente == codigoCliente);
                foreach (DocumentosAplicadosAFacturas documento in documentos)
                {
                    valor += documento.Valor ?? 0;
                }
                return valor;
            }
        }

        private bool EsFacturaConMayorSaldoCuota(List<SubFacturasxCliente> facturas, SubFacturasxCliente factura)
        {
            List<SubFacturasxCliente> nuevasFacturas = facturas.Where(x => x.NumeroCuota == factura.NumeroCuota).OrderByDescending(x => x.Saldo.Value).ToList();
            return nuevasFacturas[0].Factura == factura.Factura;
        }

        private bool ExisteFacturaCubreDescuento(List<SubFacturasxCliente> facturas, int numeroCuota, double descuentoCuota){
            IEnumerable<SubFacturasxCliente> nuevasFacturas = facturas.Where(x => x.NumeroCuota == numeroCuota);

            foreach (SubFacturasxCliente factura in nuevasFacturas)
            {
                if (decimal.ToDouble(factura.Saldo ?? 0) >= descuentoCuota)
                {
                    return true;
                }
            }

            return false;
        }

        private double CalcularDescuentoAplicar (List<SubFacturasxCliente> facturas, SubFacturasxCliente factura, double descuentoCuota){

            bool saldoCubreDescuento = decimal.ToDouble(factura.Saldo.Value) >= descuentoCuota;

            if (EsFacturaConMayorSaldoCuota(facturas, factura) && saldoCubreDescuento)
            {
                return descuentoCuota;
            }

            if (ExisteFacturaCubreDescuento(facturas, factura.NumeroCuota.Value, descuentoCuota))
            {
                return 0;
            }

            List<SubFacturasxCliente> nuevasFacturas = facturas.Where(x => x.NumeroCuota == factura.NumeroCuota).ToList();
            double descuento = descuentoCuota;

            foreach(SubFacturasxCliente e in nuevasFacturas)
            {
                SubFacturasxCliente subfactura = context.SubFacturasxCliente.FirstOrDefault(x=>x.IdFactura == e.IdFactura);
                //Si es la ultima factura de la cuota devolvemos el descuento sobrante
                if (subfactura.Factura == nuevasFacturas[nuevasFacturas.Count - 1].Factura)
                {
                    return descuento;
                }

                if (subfactura.Factura == factura.Factura)
                {
                    return descuento > decimal.ToDouble(subfactura.Saldo ?? 0) ? decimal.ToDouble(subfactura.Saldo ?? 0) : descuento;
                }

                descuento -= decimal.ToDouble(subfactura.Saldo ?? 0);
            }

            return 0;
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
                RecibosxClienteFlotanteViewModel reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel();
                reciboPost.FechaPago = new DateTime(reciboPost.FechaPago.Year, reciboPost.FechaPago.Month, reciboPost.FechaPago.Day);
                var existeRecibo = 0;
                var asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                var PagosBD = context.TiposdePago.AsNoTracking().ToList();
                var BancosBD = context.Bancos.AsNoTracking().ToList();
                var codigoCliente = "";
                int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                string inicialesAsesor = asesor.InicialesNombre;
                var subFacturas = context.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => reciboPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderBy(x => x.NumeroCuota).ThenBy(subFac => subFac.FechaVencimiento).ThenBy(x => x.Factura).ToList();
                var subFacturasCopy = context.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => reciboPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderBy(x => x.NumeroCuota).ThenBy(subFac => subFac.FechaVencimiento).ThenBy(x => x.Factura).ToList();
                List<ReciboApiModel> recibos = new List<ReciboApiModel>();
                var isOnline = EnLinea(asesor.EmpresaId, asesor.CodigoAsesor);
                Dictionary<int, double> pagadoMemory = new Dictionary<int, double>();

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
                        double Descuento = 0;
                        if (valor > 0 && noEsExponencial(valor))
                        {
                            double montoAplicado = 0;
                            double valorCuota = Decimal.ToDouble(subfactura.Saldo ?? 0);
                            var Factura = context.FacturasxCliente.Where(fa => fa.Factura == subfactura.Factura).FirstOrDefault();
                            double valorCuotaOriginal = string.IsNullOrEmpty(subfactura.IdAcuerdoxCliente) ? Decimal.ToDouble(subfactura.Saldo.Value) : Decimal.ToDouble(Factura.Saldo.Value);
                            Factura.PendienteFactura = Decimal.Parse((valor).ToString());
                            if ((valor > 0) && (valorCuota > 0))
                            {
                                bool aplicaDescuento = false;

                                if (reciboPost.Pagos[0].TipoPagoDetalle == "CH_PSF")
                                {
                                    Descuento = 0;
                                }
                                else if ((subfactura.Descuento ?? 0) == 0)
                                {
                                    DateTime FechaFact = Convert.ToDateTime(Factura.FechaFactura);

                                    if (!String.IsNullOrEmpty(subfactura.IdAcuerdoxCliente))
                                    {
                                        var acuerdo = context.AcuerdosxCliente.FirstOrDefault(a => a.IdAcuerdoxCliente == subfactura.IdAcuerdoxCliente && a.EmpresaId == subfactura.EmpresaId);
                                        if (acuerdo != null)
                                        {
                                            var GrupoDescuentoAcuerdo = context.DescuentoEnAcuerdo.FirstOrDefault(x => x.CodigoDescuento == acuerdo.GrupoDescuento && x.empresaId == acuerdo.EmpresaId);

                                            if (GrupoDescuentoAcuerdo != null)
                                            {
                                                var cuotaAcuerdo = context.CuotasXAcuerdo.FirstOrDefault(c => c.IdAcuerdoVenta == subfactura.IdAcuerdoxCliente && c.NumCuota == subfactura.NumeroCuota);
                                                
                                                if (cuotaAcuerdo != null)
                                                {
                                                    var FechaMaxDescuento = cuotaAcuerdo.FechaVencimiento;
                                                    if (FechaMaxDescuento >= reciboPost.FechaPago)
                                                    {
                                                        var documentosAplicados = context.SP_DocumentosAplicadosXCuotas(asesor.CodigoAsesor).FirstOrDefault(x => x.NumeroCuota == subfactura.NumeroCuota && x.CodigoCliente == subfactura.CodigoCliente && x.IdAcuerdoxCliente == subfactura.IdAcuerdoxCliente);
                                                        var FletePorCuota = documentosAplicados == null ? 0 : documentosAplicados.Flete;
                                                        var NotasAplicadasCuota = documentosAplicados == null ? 0 : documentosAplicados.Valor;

                                                        decimal? consumidoCuota = cuotaAcuerdo.ValorCuota - cuotaAcuerdo.SaldoDiponible;
                                                        if (!pagadoMemory.ContainsKey(cuotaAcuerdo.NumCuota))
                                                        {
                                                            pagadoMemory.Add(cuotaAcuerdo.NumCuota, 0);
                                                        }

                                                        var valoCuota = consumidoCuota - FletePorCuota - NotasAplicadasCuota ?? 0;                                                        
                                                        var pagadoCuota = context.IMObtenerPagadoCuota(subfactura.IdAcuerdoxCliente,subfactura.NumeroCuota).FirstOrDefault();
                                                        var pagadoCuotaMemory = (pagadoCuota ?? 0) + (decimal)pagadoMemory[cuotaAcuerdo.NumCuota];

                                                        var DescuentoCuota = Math.Round(Convert.ToDouble(valoCuota * (GrupoDescuentoAcuerdo.Porcentaje / 100)), 2, MidpointRounding.AwayFromZero);
                                                        Descuento = CalcularDescuentoAplicar(subFacturasCopy, subfactura, DescuentoCuota);
                                                        aplicaDescuento = valor >= (Decimal.ToDouble(valoCuota - (pagadoCuotaMemory)) - Descuento);

                                                        valorCuota = aplicaDescuento ? (Decimal.ToDouble(subfactura.Saldo.Value) - Descuento) : Decimal.ToDouble(subfactura.Saldo.Value);

                                                        pagadoMemory[cuotaAcuerdo.NumCuota] = pagadoMemory[cuotaAcuerdo.NumCuota] + valorCuota;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var cliente = context.Clientes.Where(x => x.CodigoCliente == subfactura.CodigoCliente && x.EmpresaId == subfactura.EmpresaId).FirstOrDefault();
                                        var descuento = context.Descuento.Where(x => x.Codigo == cliente.Descuento && x.EmpresaId == cliente.EmpresaId).FirstOrDefault();
                                        if (descuento != null)
                                        {
                                            var descuentoDetalle = context.DescuentoDetalle.Include(x=>x.Descuento).Where(x => x.IdLinea.ToUpper() == Factura.IdLinea.ToUpper() && x.Descuento.EmpresaId.ToUpper() ==cliente.EmpresaId.ToUpper() && x.CodigoDescuento.ToUpper() == Factura.CodigoDescuento.ToUpper()).FirstOrDefault();
                                            if (descuentoDetalle != null)
                                            {
                                                int sumaDias = (descuentoDetalle.DiasDescuento ?? 0) + cliente.DiasTransporte;
                                                var FechaMaxDescuento = FechaFact.AddDays(sumaDias);
                                                if ((FechaMaxDescuento >= reciboPost.FechaPago) || subfactura.FacturasxCliente.ExcepcionDescuento)
                                                {
                                                    var documentosAplicadosFactura = TotalDocumentosAplicados(Factura.Factura, Factura.CodigoCliente);
                                                    var valorFact = subfactura.FacturasxCliente.TotalFactura.Value - documentosAplicadosFactura - subfactura.Flete.Value;
                                                    Descuento = descuentoDetalle != null ? Math.Round(Decimal.ToDouble(valorFact) * Decimal.ToDouble(descuentoDetalle.Porcentaje.Value / 100), 2, MidpointRounding.AwayFromZero) : 0;
                                                    valorCuota = Decimal.ToDouble(subfactura.Saldo.Value) - Descuento;
                                                    aplicaDescuento = true;

                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (reciboPost.Pagos[0].TipoPagoDetalle == "CH_PSF")
                                    {
                                        Descuento = 0;
                                    }
                                    else
                                    {
                                        aplicaDescuento = ((subfactura.FechaMaxDescuento.HasValue && reciboPost.FechaPago.Date <= subfactura.FechaMaxDescuento.Value.Date) ||
                                         (subfactura.FechaVencimientoDescuento.HasValue && reciboPost.FechaPago.Date <= subfactura.FechaVencimientoDescuento.Value.Date) || subfactura.FacturasxCliente.ExcepcionDescuento);
                                        if (aplicaDescuento)
                                        {
                                            valorCuota = Decimal.ToDouble(subfactura.Saldo.Value - subfactura.Descuento.Value);
                                            Descuento = Decimal.ToDouble(subfactura.Descuento.Value);
                                        }
                                    }                                    

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
                                var recibo = recibosXPago.FirstOrDefault(rec => rec.TIPO_PAGO == pagoBD.Codigo && rec.REFERENCIA == pago.Referencia && rec.FACTURA == subfactura.Factura && rec.REF_TRANSOPEN == subfactura.Referencia);
                                RecibosxClienteViewModel reciboXCliente = recibosxCliente.FirstOrDefault(recXCli => recXCli.IdTipoPago.ToString() == pago.CodigoTipoPago && recXCli.Referencia == pago.Referencia);
                                existeRecibo = context.RecibosxCliente.Where(x => x.NumeroRecibo == reciboPost.NumeroRecibo).Count();

                                if (existeRecibo == 0)
                                {
                                    existeRecibo = context.AnticiposxCliente.Where(x => x.NumeroRecibo == reciboPost.NumeroRecibo).Count();
                                }

                                if (existeRecibo == 0)
                                {
                                    existeRecibo = context.RecibosxCliente.Where(x => x.CodigoCliente == subfactura.CodigoCliente
                                                                                 && (x.Fecha >= fechaDesde && x.Fecha <= DateTime.Now)
                                                                                 && x.IdTipoPago == pagoBD.IdTipoPago
                                                                                 && x.Referencia == pago.Referencia
                                                                                 && x.SpecPago == pago.TipoPagoDetalle
                                                                                 && x.Valor == pagoValor
                                                                                 && x.IdFactura == subfactura.IdFactura).Count();
                                }
                                if (recibo == null)
                                {
                                    recibo = new ReciboApiModel
                                    {
                                        COMPANY = subfactura.EmpresaId,
                                        ASESOR = asesor.Usuario,
                                        ASESOR_NOMBRE = asesor.Nombre,
                                        ASESOR_DIARIO = asesor.CodigoAsesor,
                                        RECIBO = reciboPost.NumeroRecibo,
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

                                    if (subfactura.EmpresaId.ToUpper() == "IMGT")
                                    {
                                        var banco = bank != null ? bank.NombreBanco : "";
                                        recibo.DESCRIPCION = $"{reciboPost.NumeroRecibo} {pago.Referencia} {banco} {recibo.FECHA} {subfactura.CodigoCliente}";
                                    }

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
                                        SpecPago = pago.TipoPagoDetalle,
                                        UsuarioCreacion = user.UserAccount,
                                        FechaCreacion = DateTime.Now,
                                        EmpresaUsuario = reciboPost.EmpresaUsuario,
                                        firmaByte = asesor.firma,
                                    };
                                    recibosxCliente.Add(reciboXCliente);
                                }
                                else if (existeRecibo > 0)
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
                                        UsuarioCreacion = user.UserAccount,
                                        FechaCreacion = DateTime.Now,
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
                                    var aplicado = detalleReciboXCliente.Valor.Value;
                                    recibo.APLICADO = aplicado.ToString();
                                    valor -= valorCuota;
                                    montoAplicado = valorCuota;
                                    if (aplicaDescuento)
                                    {
                                        detalleReciboXCliente.Descuento = Convert.ToDecimal(Descuento);
                                        recibo.DESCUENTO = (double.Parse(recibo.DESCUENTO) + Descuento).ToString();
                                    }
                                    subfactura.Saldo = 0;
                                    detalleReciboXCliente.EsAbono = false;
                                }
                                detalleReciboXCliente.ValorFactura = Factura.Saldo;
                                context.SaveChanges();

                                if (existeRecibo == 0)
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


                                var pagoAplicado = respuestaPagoRecibo.Facturas.FirstOrDefault(fact => fact.IdFactura == subfactura.Factura && string.IsNullOrEmpty(subfactura.IdAcuerdoxCliente));
                                var fechaFactura = context.FacturasxCliente.FirstOrDefault(x => x.Factura == subfactura.Factura).FechaFactura;
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
                                        cuota = subfactura.NumeroCuota
                                    };
                                    respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                                }
                                respuestaPagoRecibo.Total += montoAplicado;
                                pagoAplicado.Aplicado += montoAplicado;
                                pagoAplicado.Parcial += valorCuotaOriginal;

                                respuestaPagoRecibo.CodigoUltimoRecibo = recibo.RECIBO;
                                try
                                {
                                    pagoAplicado.Parcial2 += detalleReciboXCliente.EsAbono.Value ? 0 : Double.Parse(recibo.DESCUENTO);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    Double totalRecibo = 0;
                    recibosXPago.ForEach(recXPag => totalRecibo += Double.Parse(recXPag.APLICADO));
                    int totalFacturas = recibosXPago.GroupBy(recXPag => recXPag.FACTURA).Count();
                    recibosXPago.ForEach(recXPag =>
                    {
                        var totalRec = reciboPost.SaldoFavor > 0 ? reciboPost.SaldoFavor + totalRecibo : totalRecibo;
                        recXPag.TOTAL_APLICADO = totalRec.ToString();
                        recXPag.TOTAL_RECIBO = totalRec.ToString();
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
                    if (existeRecibo == 0)
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
                        var Esduplicado = AsyncSqlInsert.IngresarRecibos(recibosxCliente, true);

                        if (Esduplicado)
                        {
                            foreach (var recibo in recibosxCliente)
                            {
                                reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel
                                {
                                    NumeroRecibo = recibo.NumeroRecibo,
                                    CodigoCliente = recibo.CodigoCliente,
                                    Fecha = DateTime.Now,
                                    IdTipoPago = recibo.IdTipoPago,
                                    Referencia = recibo.Referencia,
                                    FechaPago = recibo.FechaPago,
                                    IdBanco = recibo.IdBanco,
                                    Valor = recibo.Valor,
                                    IdMoneda = recibo.IdMoneda,
                                    Sincronizado = false,
                                    CodigoAsesor = asesor.CodigoAsesor,
                                    IdFactura = recibo.IdFactura,
                                    Descuento = recibo.Descuento,
                                    Latitude = recibo.Latitude,
                                    Longitude = recibo.Longitude,
                                    SpecPago = recibo.SpecPago,
                                    UsuarioCreacion = recibo.UsuarioCreacion,
                                    FechaCreacion = DateTime.Now,
                                    Estado = 0, ///0: Pendiente, 1: Sincronizado, 2:Cancelado
                                    DetalleRecibo = recibo.DetalleRecibo
                                };
                                recibosxClienteFlotante.Add(reciboXClienteFlotante);
                            }
                            AsyncSqlInsert.IngresarRecibosFlotante(recibosxClienteFlotante);
                            respuestaPagoRecibo.Mensaje = "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de recibos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento de créditos para que procedan a revisar y gestionar su recibo para que sea válido.";
                            return Ok(respuestaPagoRecibo);
                        }
                        else
                        {

                            if (reciboPost.SaldoFavor > 0)
                            {
                                string ultimoValor = recibos[recibos.Count() - 1].APLICADO;
                                decimal valorConSaldoFavor = decimal.Parse(ultimoValor) + Convert.ToDecimal(reciboPost.SaldoFavor);

                                recibos[recibos.Count() - 1].APLICADO = valorConSaldoFavor.ToString();
                            }
                            var reciboHeaders = new List<ReciboApiModel>();
                            var client = new RestClient();
                            client.Timeout = 4000;
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
                                //using (AVentasEntities context = new AVentasEntities())
                                //{
                                //    asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                                //    if (reciboPost.Pagos.Count() == 1)
                                //    {
                                //        numeroCorrelativoRecibo++;
                                //        asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                                //        context.SaveChanges();
                                //    }
                                //}
                                //ValidarCorrelativoRecibo(asesor.CodigoAsesor,reciboPost.EmpresaUsuario);

                                foreach (var iter in recibos)
                                {
                                    using (AVentasEntities ctx = new AVentasEntities())
                                    {
                                        var factura = ctx.FacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA && x.EmpresaId == iter.COMPANY);

                                        if (factura != null)
                                        {
                                            var pagoAplicado = iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                                            var descuentoAplicado = iter.DESCUENTO != null ? Convert.ToDecimal(iter.DESCUENTO) : 0;
                                            factura.Saldo -= (pagoAplicado + descuentoAplicado);
                                        }

                                        var subfactura = ctx.SubFacturasxCliente.FirstOrDefault(x => x.Factura == iter.FACTURA & x.EmpresaId == iter.COMPANY & x.Referencia == iter.REF_TRANSOPEN);

                                        if (subfactura != null)
                                        {
                                            var pagoAplicado = iter.APLICADO != null ? Convert.ToDecimal(iter.APLICADO) : 0;
                                            var descuentoAplicado = iter.DESCUENTO != null ? Convert.ToDecimal(iter.DESCUENTO) : 0;
                                            subfactura.Saldo -= (pagoAplicado + descuentoAplicado);
                                        }

                                        ctx.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest(respuesta.Content);

                            }
                            string empresa = codigoCliente.Substring(0, 4);
                            // syncCuentaCorriente.SyncFacturas(empresa, codigoCliente);
                            // syncCuentaCorriente.SyncSubFacturas(empresa, codigoCliente, asesor.CodigoAsesor);

                            respuestaPagoRecibo.Mensaje = "El recibo ha sido sincronizado exitosamente.";
                            return Ok(respuestaPagoRecibo);
                        }
                    }
                    catch (Exception)
                    {
                        return BadRequest(Newtonsoft.Json.JsonConvert.SerializeObject(recibos));

                    }
                }
                else
                {
                    if (existeRecibo > 0)
                    {
                        AsyncSqlInsert.IngresarRecibosFlotante(recibosxClienteFlotante);
                        respuestaPagoRecibo.Mensaje = "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de recibos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento de créditos para que procedan a revisar y gestionar su recibo para que sea válido.";
                        return Ok(respuestaPagoRecibo);
                    }

                    var Esduplicado = AsyncSqlInsert.IngresarRecibos(recibosxCliente, false);

                    if (Esduplicado)
                    {
                        foreach (var recibo in recibosxCliente)
                        {
                            reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel
                            {
                                NumeroRecibo = recibo.NumeroRecibo,
                                CodigoCliente = recibo.CodigoCliente,
                                Fecha = DateTime.Now,
                                IdTipoPago = recibo.IdTipoPago,
                                Referencia = recibo.Referencia,
                                FechaPago = recibo.FechaPago,
                                IdBanco = recibo.IdBanco,
                                Valor = recibo.Valor,
                                IdMoneda = recibo.IdMoneda,
                                Sincronizado = false,
                                CodigoAsesor = asesor.CodigoAsesor,
                                IdFactura = recibo.IdFactura,
                                Descuento = recibo.Descuento,
                                Latitude = recibo.Latitude,
                                Longitude = recibo.Longitude,
                                SpecPago = recibo.SpecPago,
                                UsuarioCreacion = recibo.UsuarioCreacion,
                                FechaCreacion = DateTime.Now,
                                Estado = 0, ///0: Pendiente, 1: Sincronizado, 2:Cancelado
                                DetalleRecibo = recibo.DetalleRecibo
                            };
                            recibosxClienteFlotante.Add(reciboXClienteFlotante);
                        }
                        AsyncSqlInsert.IngresarRecibosFlotante(recibosxClienteFlotante);
                        respuestaPagoRecibo.Mensaje = "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de recibos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento de créditos para que procedan a revisar y gestionar su recibo para que sea válido.";
                        return Ok(respuestaPagoRecibo);
                    }
                    else
                    {
                        //using (AVentasEntities ctx = new AVentasEntities())
                        //{
                        //    asesor = ctx.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                        //    if (reciboPost.Pagos.Count() == 1)
                        //    {
                        //        numeroCorrelativoRecibo++;
                        //        asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                        //        ctx.SaveChanges();
                        //    }
                        //}
                        //ValidarCorrelativoRecibo(asesor.CodigoAsesor,reciboPost.EmpresaUsuario);


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
                    }
                    respuestaPagoRecibo.Mensaje = "El recibo ha sido sincronizado exitosamente.";
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

                        syncCuentaCorriente.SyncFacturas(recibos[0].COMPANY, recibos[0].CLIENTE,recibos[0].ASESOR);
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

                        syncCuentaCorriente.SyncFacturas(recibos[0].COMPANY, recibos[0].CLIENTE, recibos[0].ASESOR);
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
        [Route("~/api/Recibo/flotante/{FechaInicio}/{FechaFin}/{estado}/{asesor}")]
        public IHttpActionResult GetFlotantes(DateTime FechaInicio, DateTime FechaFin, int estado,string asesor)
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
                    
                    List<RecibosxClienteViewModel> Recibos = ctx.RecibosxClienteFlotante.Where(r => r.Estado == estado && r.Fecha >= FechaInicio && r.Fecha < FechaFin && r.CodigoAsesor==asesor).Select(rec => new RecibosxClienteViewModel
                    {
                        Id = rec.ReciboId,
                        ReciboGenerado = rec.ReciboIdGenerado == null ? "No Disponible" : rec.ReciboIdGenerado,
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

        [HttpGet]
        [Route("~/api/Recibo/flotante/{FechaInicio}/{FechaFin}/{estado}/{asesor}")]
        public async Task<IHttpActionResult> GetFlotantesAsesores(DateTime FechaInicio, DateTime FechaFin, int estado)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x => x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
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

                    List<RecibosxClienteViewModel> RecibosFlotantes = new List<RecibosxClienteViewModel>();

                    foreach(var asesor in asesoresHabilitados.Distinct().ToList())
                    {
                        List<RecibosxClienteViewModel> Recibos = ctx.RecibosxClienteFlotante.Where(r => r.Estado == estado && r.Fecha >= FechaInicio && r.Fecha < FechaFin && r.CodigoAsesor == asesor).Select(rec => new RecibosxClienteViewModel
                        {
                            Id = rec.ReciboId,
                            ReciboGenerado = rec.ReciboIdGenerado == null ? "No Disponible" : rec.ReciboIdGenerado,
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


                        RecibosFlotantes.AddRange(Recibos);
                    }

                    return Ok(RecibosFlotantes);
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

                    var asesor = await ctx.Asesores.FirstOrDefaultAsync(ase => ase.Usuario == recibo.CodigoAsesor && ase.CorrelativoRecibos!=null);
                    int numeroCorelativo = asesor.CorrelativoRecibos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";

                    RecibosxCliente reciboBD = null;
                    AnticiposxCliente anticipoBD = null;

                    if (recibo.Tipo != null || recibo.IdFactura == null)
                    {
                        anticipoBD = GenerarAnticipo(recibo, numeroReferencia,asesor);
                        ctx.AnticiposxCliente.Add(anticipoBD);
                    }
                    else
                    {
                        reciboBD = GenerarRecibo(recibo, numeroReferencia,asesor);
                        ctx.RecibosxCliente.Add(reciboBD);
                    }

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

                    if (EnLinea(asesor.EmpresaId, asesor.CodigoAsesor))
                    {
                        List<ReciboApiModel> ReciboSincronizar = new List<ReciboApiModel>();

                        if (recibo.Tipo != null || recibo.IdFactura == null)
                        {
                            ReciboSincronizar = GenerarAnticipoApiModels(anticipoBD, asesor);
                        }
                        else
                        {
                            ReciboSincronizar = GenerarReciboApiModels(reciboBD, asesor);
                        }

                        try
                        {
                            var client = new RestClient();
                            var request = new RestRequest($"{Enviroment.CRMWebServiceURLApi}recibos/upload", Method.POST)
                            {
                                RequestFormat = DataFormat.Json
                            };
                            request.AddHeader("Content-type", "application/json; charset=utf-8");
                            request.Parameters.Clear();
                            request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(ReciboSincronizar), ParameterType.RequestBody);
                            var respuesta = client.Execute(request);

                            if (respuesta.IsSuccessful && respuesta.Content.Equals("\"\""))
                            {
                                if (recibo.Tipo != null || recibo.IdFactura == null)
                                {
                                    var reciboBDA = await ctx.AnticiposxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo == numeroReferencia);
                                    reciboBDA.Sincronizado = true;
                                    await ctx.SaveChangesAsync();
                                }
                                else
                                {
                                    var reciboBDA = await ctx.RecibosxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo == numeroReferencia);
                                    reciboBDA.Sincronizado = true;
                                    await ctx.SaveChangesAsync();
                                }

                                syncCuentaCorriente.SyncFacturas(ReciboSincronizar[0].COMPANY, ReciboSincronizar[0].CLIENTE, ReciboSincronizar[0].ASESOR);
                                syncCuentaCorriente.SyncSubFacturas(ReciboSincronizar[0].COMPANY, ReciboSincronizar[0].CLIENTE, ReciboSincronizar[0].ASESOR);
                                return Ok($"El recibo ha sido aprobado y sincronizado exitosamente con AX.");
                            }
                            else
                            {
                                return Ok($"El recibo ha sido aprobado pero pendiente de sincronización con AX.");
                            }
                        }
                        catch (Exception e)
                        {
                            return Ok($"El recibo ha sido aprobado pero pendiente de sincronización con AX.");
                        }
                    }
                    else
                    {
                        return Ok("El recibo ha sido aprobado pero pendiente de sincronización con AX.");
                    }

                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Recibo/obtenerfirma/{recibo}")]
        public async Task<IHttpActionResult> ObtenerFirmaRecibo(string recibo)
        {
            try
            {
                using(AVentasEntities entities = new AVentasEntities())
                {
                    var reciboBD = await entities.RecibosxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo.ToUpper() == recibo.ToUpper());
                    if (reciboBD != null)
                    {
                        var asesor = await entities.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor.ToUpper() == reciboBD.UsuarioCreacion.ToUpper());
                        if (asesor != null)
                        {
                            string firma = "";
                            if (asesor.firma != null)
                            {
                                firma = "data:image/png;base64," + Convert.ToBase64String(asesor.firma);
                            }

                            return Ok(new { nombreAsesor=asesor.Nombre,firma=firma});
                        }
                    }

                    var anticipoBd = await entities.AnticiposxCliente.FirstOrDefaultAsync(x => x.NumeroRecibo.ToUpper() == recibo.ToUpper());
                    if (anticipoBd != null)
                    {
                        var asesor = await entities.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor.ToUpper() == anticipoBd.UsuarioCreacion.ToUpper());
                        if (asesor != null)
                        {
                            string firma = "";
                            if (asesor.firma != null)
                            {
                                firma = "data:image/png;base64," + Convert.ToBase64String(asesor.firma);
                            }

                            return Ok(new { nombreAsesor = asesor.Nombre, firma = firma });
                        }
                    }

                    return NotFound();
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private RecibosxCliente GenerarRecibo(RecibosxClienteFlotante reciboFlotante, string numeroReferencia,Asesores asesor)
        {
            RecibosxCliente reciboBD = new RecibosxCliente()
            {
                NumeroRecibo = numeroReferencia,
                CodigoCliente = reciboFlotante.CodigoCliente,
                Fecha = reciboFlotante.Fecha,
                IdTipoPago = reciboFlotante.IdTipoPago,
                SpecPago = reciboFlotante.SpecPago,
                Referencia = reciboFlotante.Referencia,
                FechaCheque = reciboFlotante.FechaCheque,
                IdBanco = reciboFlotante.IdBanco,
                IdCuentaBancaria = reciboFlotante.IdCuentaBancaria,
                Valor = reciboFlotante.Valor,
                IdMoneda = reciboFlotante.IdMoneda,
                CodigoAsesor = reciboFlotante.CodigoAsesor,
                FechaCreacion = reciboFlotante.FechaCreacion,
                UsuarioCreacion = reciboFlotante.UsuarioCreacion,
                FechaModificacion = reciboFlotante.FechaModificacion,
                UsuarioModificacion = reciboFlotante.UsuarioModificacion,
                IdFactura = reciboFlotante.IdFactura,
                Descuento = reciboFlotante.Descuento,
                Latitude = reciboFlotante.Latitude,
                Longitude = reciboFlotante.Longitude,
                Sincronizado = false,
                firma=asesor.firma,
                RecibosDetalle = reciboFlotante.RecibosDetalleFlotante.Select(d => new RecibosDetalle()
                {
                    IdReciboDetalle = d.IdReciboDetalle,
                    ReciboId = d.ReciboId,
                    IdSubFactura = d.IdSubFactura,
                    Valor = d.Valor,
                    Descuento = d.Descuento,
                    EsAbono = d.EsAbono,
                }).ToList()
            };

            return reciboBD;
        }
        private AnticiposxCliente GenerarAnticipo(RecibosxClienteFlotante reciboFlotante, string numeroReferencia,Asesores asesor)
        {
            AnticiposxCliente reciboBD = new AnticiposxCliente()
            {
                NumeroRecibo = numeroReferencia,
                CodigoCliente = reciboFlotante.CodigoCliente,
                Fecha = reciboFlotante.Fecha,
                IdTipoPago = reciboFlotante.IdTipoPago,
                SpecPago = reciboFlotante.SpecPago,
                Referencia = reciboFlotante.Referencia,
                FechaCheque = reciboFlotante.FechaCheque,
                IdBanco = reciboFlotante.IdBanco,
                IdCuentaBancaria = reciboFlotante.IdCuentaBancaria,
                Valor = reciboFlotante.Valor,
                IdMoneda = reciboFlotante.IdMoneda,
                CodigoAsesor = reciboFlotante.CodigoAsesor,
                FechaCreacion = reciboFlotante.FechaCreacion,
                UsuarioCreacion = reciboFlotante.UsuarioCreacion,
                FechaModificacion = reciboFlotante.FechaModificacion,
                UsuarioModificacion = reciboFlotante.UsuarioModificacion,
                Descuento = reciboFlotante.Descuento,
                Latitude = reciboFlotante.Latitude,
                Longitude = reciboFlotante.Longitude,
                Sincronizado = false,
                Tipo = reciboFlotante.Tipo,
                EsContado = reciboFlotante.EsContado.Value,
                NumPedido = reciboFlotante.NumPedido,
                firma=asesor.firma
            };

            return reciboBD;
        }
        private List<ReciboApiModel> GenerarReciboApiModels(RecibosxCliente recibo, Asesores asesor)
        {
            using (AVentasEntities ctx = new AVentasEntities())
            {
                List<ReciboApiModel> ReciboSincronizar = new List<ReciboApiModel>();
                var ReciboDetalle = ctx.RecibosDetalle.Where(s => s.RecibosxCliente.NumeroRecibo == recibo.NumeroRecibo).ToList();
                var TipoPago = ctx.TiposdePago.Where(a => a.IdTipoPago == recibo.IdTipoPago).FirstOrDefault();
                var Banco = ctx.Bancos.Where(a => a.IdBanco == recibo.IdBanco).FirstOrDefault();

                foreach (var Detalle in ReciboDetalle)
                {
                    var Recibos = new ReciboApiModel
                    {
                        COMPANY = asesor.EmpresaId,
                        ASESOR = asesor.Usuario,
                        ASESOR_NOMBRE = asesor.Nombre,
                        ASESOR_DIARIO = asesor.CodigoAsesor,
                        RECIBO = recibo.NumeroRecibo,
                        CLIENTE = recibo.CodigoCliente,
                        MONEDA = recibo.IdMoneda,
                        FECHA = recibo.Fecha.Value.ToString("dd/MM/yyyy"),
                        DESCRIPCION = "",
                        TOTAL_RECIBO = recibo.Valor.ToString(),
                        TOTAL_FACTURAS = ReciboDetalle.Count().ToString(),
                        TOTAL_APLICADO = recibo.Valor.ToString(),
                        TIPO_PAGO = TipoPago.Codigo,
                        SPEC_PAGO = recibo.SpecPago,
                        BANCO = Banco != null ? Banco.NombreBanco : "",
                        REFERENCIA = recibo.Referencia,
                        FECHA_PAGO = recibo.FechaCheque.Value.ToString("dd/MM/yyyy"),
                        FACTURA = Detalle.SubFacturasxCliente.Factura,
                        APLICADO = Detalle.Valor.ToString(),
                        DESCUENTO = Detalle.Descuento.ToString(),
                        REF_TRANSOPEN = Detalle.SubFacturasxCliente.Referencia,
                        ES_CONTADO = "0",
                        NUM_PEDIDO = "",
                    };
                    ReciboSincronizar.Add(Recibos);
                }

                return ReciboSincronizar;
            }
        }
        private List<ReciboApiModel> GenerarAnticipoApiModels(AnticiposxCliente recibo, Asesores asesor)
        {
            using (AVentasEntities ctx = new AVentasEntities())
            {
                List<ReciboApiModel> ReciboSincronizar = new List<ReciboApiModel>();
                var TipoPago = ctx.TiposdePago.Where(a => a.IdTipoPago == recibo.IdTipoPago).FirstOrDefault();
                var Banco = ctx.Bancos.Where(a => a.IdBanco == recibo.IdBanco).FirstOrDefault();

                var Recibos = new ReciboApiModel
                {
                    COMPANY = asesor.EmpresaId,
                    ASESOR = asesor.Usuario,
                    ASESOR_NOMBRE = asesor.Nombre,
                    ASESOR_DIARIO = asesor.CodigoAsesor,
                    RECIBO = recibo.NumeroRecibo,
                    CLIENTE = recibo.CodigoCliente,
                    MONEDA = recibo.IdMoneda,
                    FECHA = recibo.Fecha.Value.ToString("dd/MM/yyyy"),
                    DESCRIPCION = "",
                    TOTAL_RECIBO = recibo.Valor.ToString(),
                    TOTAL_FACTURAS = recibo.Valor.ToString(),
                    TOTAL_APLICADO = recibo.Valor.ToString(),
                    TIPO_PAGO = TipoPago.Codigo,
                    SPEC_PAGO = recibo.SpecPago,
                    BANCO = Banco != null ? Banco.NombreBanco : "",
                    REFERENCIA = recibo.Referencia,
                    FECHA_PAGO = recibo.FechaCheque.Value.ToString("dd/MM/yyyy"),
                    FACTURA = "Anticipo",
                    APLICADO = recibo.Valor.ToString(),
                    DESCUENTO = recibo.Descuento.ToString(),
                    REF_TRANSOPEN = "",
                    ES_CONTADO = recibo.EsContado.ToString(),
                    NUM_PEDIDO = recibo.NumPedido,
                };
                ReciboSincronizar.Add(Recibos);

                return ReciboSincronizar;
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
        public string Mensaje { get; set; }
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
        public int? cuota { get; set; }
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
