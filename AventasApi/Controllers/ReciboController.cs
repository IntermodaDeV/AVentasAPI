using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using AventasApi.Services.Authentication;
using AventasApi.Services.AsyncJobs;
using AventasApi.Enviroments;
using AventasApi.Models;

namespace AventasApi.Controllers
{
    public class ReciboController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public ReciboController()
        {
            _authenticationAppService = new AuthenticationAppService();

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
                IdMoneda = rec.IdMoneda,
                Sincronizado = rec.Sincronizado,
                CodigoAsesor = rec.CodigoAsesor,
                IdFactura = rec.IdFactura,
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
                DetalleRecibo = rec.RecibosDetalle.Select(recDet => new RecibosDetalleViewModel
                {
                    IdReciboDetalle = recDet.IdReciboDetalle,
                    Factura = rec.FacturasxCliente.Factura,
                    Tipo = rec.FacturasxCliente.Tipo,
                    ReciboId = recDet.ReciboId,
                    IdSubFactura = recDet.IdSubFactura,
                    Valor = recDet.Valor,
                    ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                    Descuento = recDet.Descuento,
                    DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                }).ToList()
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
                DetalleRecibo = new List<RecibosDetalleViewModel> { new RecibosDetalleViewModel {
                    Valor = ant.Valor,
                    ValorSinDescuento = ant.Valor,
                    DiasVencimiento = 0,
                    Tipo = ant.Tipo,
                    Factura = "Anticipo",
                } },
                //rec.RecibosDetalle.Select(recDet => new RecibosDetalleViewModel
                //{
                //    IdReciboDetalle = recDet.IdReciboDetalle,
                //    Factura = rec.FacturasxCliente.Factura,
                //    Tipo = rec.FacturasxCliente.Tipo,
                //    ReciboId = recDet.ReciboId,
                //    IdSubFactura = recDet.IdSubFactura,
                //    Valor = recDet.Valor,
                //    ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                //    Descuento = recDet.Descuento,
                //    DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                //}).ToList()
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
            if (anticipoPost.Pagos != null && anticipoPost.Pagos.Count == 1)
            {
                var asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);
                var pago = anticipoPost.Pagos[0];
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
                    NumeroRecibo = $"{inicialesAsesor}-1{numeroCorrelativoRecibo.ToString("D5")}"
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

                return Ok(respuestaPagoRecibo);
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IHttpActionResult> PostRecibo(ReciboPostViewModel reciboPost)
        {
            //var user = new { UserAccount = "gmonrroy" };
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);


            RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
            List<RecibosxClienteViewModel> recibosxCliente = new List<RecibosxClienteViewModel>();
            reciboPost.FechaPago = new DateTime(reciboPost.FechaPago.Year, reciboPost.FechaPago.Month, reciboPost.FechaPago.Day);
            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            var asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
            var PagosBD = context.TiposdePago.AsNoTracking().ToList();
            var BancosBD = context.Bancos.AsNoTracking().ToList();
            int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
            string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);
            var subFacturas = context.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => reciboPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderByDescending(subFac => subFac.FechaVencimiento).ToList();
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
                        //DateTime fechaDescuento = new DateTime(subfactura.FechaVencimientoDescuento.Value.Year, subfactura.FechaVencimientoDescuento.Value.Month, subfactura.FechaVencimientoDescuento.Value.Day);
                        //if (subfactura.IdAcuerdoxCliente != null && (new DateTime(subfactura.FechaMaxDescuento.Value.Year, subfactura.FechaMaxDescuento.Value.Month, subfactura.FechaMaxDescuento.Value.Day) > new DateTime(2000, 1, 1)))
                        //    fechaDescuento = new DateTime(subfactura.FechaMaxDescuento.Value.Year, subfactura.FechaMaxDescuento.Value.Month, subfactura.FechaMaxDescuento.Value.Day);


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
                        var recibo = recibosXPago.FirstOrDefault(rec => rec.TIPO_PAGO == pago.CodigoTipoPago && rec.REFERENCIA == pago.Referencia);
                        RecibosxClienteViewModel reciboXCliente = recibosxCliente.FirstOrDefault(recXCli => recXCli.IdTipoPago.ToString() == pago.CodigoTipoPago && recXCli.Referencia == pago.Referencia);
                        if (recibo == null)
                        {
                            recibo = new ReciboApiModel
                            {
                                COMPANY = subfactura.EmpresaId,
                                ASESOR = asesor.Usuario,
                                ASESOR_NOMBRE = asesor.Nombre,
                                ASESOR_DIARIO = asesor.CodigoAsesor,
                                RECIBO = $"{inicialesAsesor}-1{numeroCorrelativoRecibo.ToString("D5")}",//numeroRecibo.ToString(),
                                CLIENTE = subfactura.CodigoCliente,
                                MONEDA = pago.IdMoneda,
                                FECHA = DateTime.Now.ToString("dd/MM/yyyy"),//reciboPost.Fecha.ToString("dd/MM/yyyy"),
                                DESCRIPCION = reciboPost.Descripcion,
                                //TOTAL_RECIBO = item.Valor.ToString(),
                                //TOTAL_FACTURAS = facturas.Count.ToString(),
                                //TOTAL_APLICADO = facturas.Sum(f => f.Valor).ToString(),
                                TIPO_PAGO = pago.CodigoTipoPago,
                                //SPEC_PAGO = ? ,
                                BANCO = pago.IdBanco,
                                REFERENCIA = pago.Referencia,
                                FECHA_PAGO = reciboPost.FechaPago.ToString("dd/MM/yyyy"),
                                FACTURA = subfactura.Factura,
                                APLICADO = "0",
                                DESCUENTO = "0",
                                REF_TRANSOPEN = pago.ReferenciaTransaccionAbierta,
                            };
                            numeroCorrelativoRecibo++;
                            recibosXPago.Add(recibo);
                        }
                        if (reciboXCliente == null)
                        {
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
                                Descuento = 0
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
                            montoAplicado = valor;
                            valor = 0;
                        }
                        else
                        {
                            detalleReciboXCliente.Valor = Decimal.Parse((valorCuota + aplicadoDouble).ToString());

                            recibo.APLICADO = detalleReciboXCliente.Valor.ToString();
                            valor -= valorCuota;
                            montoAplicado = valorCuota;
                            if (aplicaDescuento)
                            {
                                detalleReciboXCliente.Descuento = subfactura.Descuento;
                                recibo.DESCUENTO = (decimal.Parse(recibo.DESCUENTO) + subfactura.Descuento).ToString();
                            }
                            subfactura.Saldo = 0;
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
                                Fecha = subfactura.FechaVencimiento.Value,
                                Dias = dias,
                                TipoDocumento = subfactura.FacturasxCliente.Tipo,

                            };
                            respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                        }
                        respuestaPagoRecibo.Total += montoAplicado;
                        pagoAplicado.Aplicado += montoAplicado;
                        pagoAplicado.Parcial += valorCuotaOriginal;

                        respuestaPagoRecibo.CodigoUltimoRecibo = $"{inicialesAsesor}-1{numeroCorrelativoRecibo.ToString("D5")}";
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
            //return Ok(recibos);
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
                //request.AddBody(recibos);
                var respuesta = client.Execute(request);

                if (respuesta.IsSuccessful && respuesta.Content.Equals("\"\""))
                {
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                        asesor.CorrelativoRecibos = numeroCorrelativoRecibo;
                        context.SaveChanges();
                    }
                    AsyncSqlInsert.IngresarRecibos(recibosxCliente);
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
            return Ok(recibos);
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
        public DateTime Fecha { get; set; }
        public double Parcial { get; set; }
        public double Aplicado { get; set; }
        public double Parcial2 { get; set; }
        public int Dias { get; set; }
        public string TipoDocumento { get; set; }
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
