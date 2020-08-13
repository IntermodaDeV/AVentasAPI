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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

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
        public IHttpActionResult Get()
        {
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
            var recibosXAsesor = context.RecibosxCliente.Where(recCli => recCli.CodigoAsesor == user.UserAccount).Select(rec => new RecibosxClienteViewModel
            {

                NumeroRecibo = rec.NumeroRecibo,
                CodigoCliente = rec.CodigoCliente,
                Fecha = rec.Fecha,
                IdTipoPago = rec.IdTipoPago,
                Referencia = rec.Referencia,
                FechaPago = rec.FechaCheque,
                IdBanco = rec.IdBanco,
                Valor = rec.Valor,
                IdMoneda = context.MaestroMoneda.FirstOrDefault(x=>x.IdMoneda==rec.IdMoneda).Moneda,
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
                    Tipo = rec.FacturasxCliente.Tipo,
                    ReciboId = recDet.ReciboId,
                    IdSubFactura = recDet.IdSubFactura,
                    Valor = recDet.Valor,
                    ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                    Descuento = recDet.Descuento,
                    EsAbono = recDet.EsAbono,
                    DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                } : new RecibosDetalleViewModel
                {
                    IdReciboDetalle = recDet.IdReciboDetalle,
                    Factura = "SALDO_FAVOR",
                    NumeroFel = "",
                    Tipo = "Pago",
                    ReciboId = recDet.ReciboId,
                    IdSubFactura = null,
                    Valor = recDet.Valor,
                    ValorSinDescuento = recDet.Valor,
                    Descuento = 0,
                    EsAbono = true,
                    DiasVencimiento = 0,
                }
                ).ToList()
            }).ToList();
            var anticiposXAsesor = context.AnticiposxCliente.Where(recCli => recCli.CodigoAsesor == user.UserAccount).Select(ant => new RecibosxClienteViewModel
            {

                NumeroRecibo = ant.NumeroRecibo,
                CodigoCliente = ant.CodigoCliente,
                Fecha = ant.Fecha,
                IdTipoPago = ant.IdTipoPago,
                Referencia = ant.Referencia,
                FechaPago = ant.FechaCheque,
                IdBanco = ant.IdBanco,
                Valor = ant.Valor,
                IdMoneda = ant.IdMoneda,
                Sincronizado = ant.Sincronizado,
                CodigoAsesor = ant.CodigoAsesor,
                IdFactura = 0,
                Latitude=ant.Latitude,
                Longitude = ant.Longitude,
                DescripcionBanco = context.Bancos.Where(banco => banco.IdBanco == ant.IdBanco).Select(banco => banco.Descripcion).FirstOrDefault(),
                Descuento = ant.Descuento,
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
                } }
            }).ToList();
            recibosXAsesor.AddRange(anticiposXAsesor);
            return Ok(recibosXAsesor);
        }
        [Route("api/Recibo/Anticipo")]
        [HttpPost]
        public async Task<IHttpActionResult> PostAnticipo(ReciboPostViewModel anticipoPost)
        {
            RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();

            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
            if (anticipoPost.Pagos != null)
            {
                List<ReciboApiModel> recibos = new List<ReciboApiModel>();
                foreach (var pag in anticipoPost.Pagos)
                {

                    var asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                    int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                    string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);
                    var pago = pag;
                    int.TryParse(pago.IdBanco, out int codigobanco);
                    decimal.TryParse(pago.Valor.ToString(), out decimal valorPago);
                    var anticipo = new AnticiposxCliente
                    {
                        CodigoCliente = anticipoPost.CodigoCliente,
                        Fecha = anticipoPost.Fecha,
                        IdTipoPago = int.Parse(pago.CodigoTipoPago),
                        Referencia = pago.Referencia,
                        FechaCheque = anticipoPost.FechaPago,
                        IdBanco = codigobanco,
                        //IdCuentaBancaria = ,
                        Valor = valorPago,
                        IdMoneda = pago.IdMoneda,
                        CodigoAsesor = user.UserAccount,
                        Tipo = anticipoPost.Tipo,
                        NumeroRecibo = $"{inicialesAsesor}-1{numeroCorrelativoRecibo.ToString("D5")}",
                        NumPedido = anticipoPost.NumPedido,
                        Latitude = (anticipoPost.location != null)  ? anticipoPost.location.latitude : null,
                        Longitude = (anticipoPost.location != null) ? anticipoPost.location.longitude : null
                    };
                    var pagoBD = context.TiposdePago.FirstOrDefault(pa => pa.IdTipoPago.ToString() == pago.CodigoTipoPago);
                    var respuestapago = new RespuestaPago
                    {
                        TipoPago = pagoBD.Descripcion,
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
                    context.AnticiposxCliente.Add(anticipo);
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
                    respuestaPagoRecibo.CodigoUltimoRecibo = anticipo.NumeroRecibo;
                    respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                    context.SaveChanges();
                    ReciboApiModel anticipoAX = new ReciboApiModel
                    {
                        COMPANY = "IMHN",
                        ASESOR = asesor.Usuario,
                        ASESOR_NOMBRE = asesor.Nombre,
                        ASESOR_DIARIO = asesor.CodigoAsesor,
                        RECIBO = anticipo.NumeroRecibo,
                        CLIENTE = anticipo.CodigoCliente,
                        MONEDA = pago.IdMoneda,
                        FECHA = DateTime.Now.ToString("dd/MM/yyyy"),
                        DESCRIPCION = anticipoPost.Descripcion,
                        TOTAL_RECIBO = valorPago.ToString(),
                        TOTAL_FACTURAS = valorPago.ToString(),
                        TOTAL_APLICADO = valorPago.ToString(),
                        TIPO_PAGO = pago.CodigoTipoPago,
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
                    return Ok(respuestaPagoRecibo);
                }
                else
                {
                    return BadRequest(respuesta.Content);
                }
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IHttpActionResult> PostRecibo(ReciboPostViewModel reciboPost)
        {
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);


            RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
            List<RecibosxClienteViewModel> recibosxCliente = new List<RecibosxClienteViewModel>();
            reciboPost.FechaPago = new DateTime(reciboPost.FechaPago.Year, reciboPost.FechaPago.Month, reciboPost.FechaPago.Day);
            var asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
            var PagosBD = context.TiposdePago.AsNoTracking().ToList();
            var BancosBD = context.Bancos.AsNoTracking().ToList();
            var codigoCliente = "";
            int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
            string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);
            var subFacturas = context.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => reciboPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderBy(subFac => subFac.FechaVencimiento).ToList();
            List<ReciboApiModel> recibos = new List<ReciboApiModel>();
            foreach (PagosReciboPostViewModel pago in reciboPost.Pagos.OrderBy(pag => pag.Orden))
            {
                var pagoBD = PagosBD.FirstOrDefault(pa => pa.IdTipoPago.ToString() == pago.CodigoTipoPago);
                var respuestapago = new RespuestaPago
                {
                    TipoPago = pagoBD.Descripcion,
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
                        var recibo = recibosXPago.FirstOrDefault(rec => rec.TIPO_PAGO == pago.CodigoTipoPago && rec.REFERENCIA == pago.Referencia && rec.FACTURA == subfactura.Factura);
                        RecibosxClienteViewModel reciboXCliente = recibosxCliente.FirstOrDefault(recXCli => recXCli.IdTipoPago.ToString() == pago.CodigoTipoPago && recXCli.Referencia == pago.Referencia);
                        if (recibo == null)
                        {
                            recibo = new ReciboApiModel
                            {
                                COMPANY = subfactura.EmpresaId,
                                ASESOR = asesor.Usuario,
                                ASESOR_NOMBRE = asesor.Nombre,
                                ASESOR_DIARIO = asesor.CodigoAsesor,
                                RECIBO = $"{inicialesAsesor}-1{numeroCorrelativoRecibo.ToString("D5")}",
                                CLIENTE = subfactura.CodigoCliente,
                                MONEDA = pago.IdMoneda,
                                FECHA = DateTime.Now.ToString("dd/MM/yyyy"),
                                DESCRIPCION = reciboPost.Descripcion,
                                TIPO_PAGO = pagoBD.Codigo,
                                SPEC_PAGO = pago.TipoPagoDetalle,
                                BANCO = bank != null ? bank.NombreBanco :"",
                                REFERENCIA = pago.Referencia,
                                FECHA_PAGO = reciboPost.FechaPago.ToString("dd/MM/yyyy"),
                                FACTURA = subfactura.Factura,
                                APLICADO = "0",
                                DESCUENTO = "0",
                                REF_TRANSOPEN = subfactura.Referencia,
                            };
                            recibosXPago.Add(recibo);
                            if (reciboPost.Pagos.Count() > 1)
                                numeroCorrelativoRecibo++;
                        }
                        if (reciboXCliente == null)
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
                                Sincronizado = true,
                                CodigoAsesor = asesor.CodigoAsesor,
                                IdFactura = subfactura.IdFactura,
                                Descuento = 0,
                                Latitude = (reciboPost.location != null) ? reciboPost.location.latitude:null,
                                Longitude= (reciboPost.location != null) ? reciboPost.location.longitude:null
                            };
                            recibosxCliente.Add(reciboXCliente);
                        }
                        double aplicadoDouble = 0;
                        double.TryParse(recibo.APLICADO, out aplicadoDouble);
                        RecibosDetalleViewModel detalleReciboXCliente = new RecibosDetalleViewModel
                        {
                            IdSubFactura = subfactura.IdSubFactura,
                            Descuento = 0
                        };
                        if (valorCuota > valor)
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
                        reciboXCliente.Descuento += detalleReciboXCliente.Descuento;
                        reciboXCliente.Valor += detalleReciboXCliente.Valor;
                        reciboXCliente.DetalleRecibo.Add(detalleReciboXCliente);
                        var pagoAplicado = respuestaPagoRecibo.Facturas.FirstOrDefault(fact => fact.IdFactura == recibo.FACTURA);
                        if (pagoAplicado == null)
                        {
                            TimeSpan ts = reciboPost.Fecha - subfactura.FechaVencimiento.Value;

                            int dias = ts.Days;

                            pagoAplicado = new RespuestaFactura
                            {
                                IdFactura = recibo.FACTURA,
                                NumeroFEL = subfactura.NumeroFEL,
                                Fecha = subfactura.FechaVencimiento.Value,
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
                var primerRecibo = recibosxCliente.FirstOrDefault();
                primerRecibo.Valor += decimal.Parse(reciboPost.SaldoFavor.ToString());
                primerRecibo.DetalleRecibo.Add(new RecibosDetalleViewModel
                {
                    Valor = decimal.Parse(reciboPost.SaldoFavor.ToString()),
                    ValorSinDescuento = decimal.Parse(reciboPost.SaldoFavor.ToString())
                });
            }
            
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
                            numeroCorrelativoRecibo++;
                        asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                        context.SaveChanges();
                    }
                    AsyncSqlInsert.IngresarRecibos(recibosxCliente);
                    syncCuentaCorriente.SyncFacturas(asesor.EmpresaId, codigoCliente);
                    syncCuentaCorriente.SyncSubFacturas(asesor.EmpresaId,codigoCliente, asesor.CodigoAsesor);
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
        public DateTime Fecha { get; set; }
        public string Banco { get; set; }
        public string Referencia { get; set; }
        public double Monto { get; set; }
    }
}
