using AventasApi.Models;
using AventasApi.Models.ViewModels;
using AventasApi.Services.AsyncJobs;
using AventasApi.Services.Authentication;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
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
    [RoutePrefix("api/proforma")]
    public class ProformaController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public ProformaController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        public void EscribirEnArchivo(string Message)
        {
            try
            {
                #region Creacion Carpeta
                string path = @"C:\AVentasAPIProforma";
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

        [HttpGet]
        [Route("{asesor}/{FechaInicio}/{FechaFin}")]
        public async Task<IHttpActionResult> ObtenerProformas(string Asesor,DateTime FechaInicio,DateTime FechaFin)
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
                        asesoresHabilitados = await ctx.Asesores.Where(x => x.CodigoAsesor == Asesor).Select(x => x.CodigoAsesor).ToListAsync();
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
                        var Recibos = ctx.RecibosProforma.Where(r => r.CodigoAsesor == asesor && r.Fecha >= FechaInicio && r.Fecha < FechaFin).Select(rec => new RecibosxClienteViewModel
                        {
                            Anticipo = false,
                            NombreAsesor = ctx.Asesores.FirstOrDefault(x => x.CodigoAsesor == rec.CodigoAsesor).Nombre,
                            Asesor = rec.CodigoAsesor,
                            NumeroRecibo = rec.NumeroProforma,
                            CodigoCliente = rec.CodigoCliente,
                            Fecha = rec.Fecha,
                            IdTipoPago = rec.IdTipoPago,
                            Referencia = rec.Referencia,
                            FechaPago = rec.FechaCheque,
                            IdBanco = rec.IdBanco,
                            Valor = rec.Valor,
                            IdMoneda = ctx.MaestroMoneda.FirstOrDefault(x => x.IdMoneda == rec.IdMoneda).Moneda,
                            CodigoAsesor = rec.CodigoAsesor,
                            IdFactura = rec.IdFactura,
                            Longitude = rec.longitude,
                            Latitude = rec.latitude,
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
                            DetalleRecibo = rec.RecibosProformaDetalle.Select(recDet =>
                            recDet.SubFacturasxCliente != null ?
                            new RecibosDetalleViewModel
                            {
                                IdReciboDetalle = recDet.IdProformaDetalle,
                                Factura = recDet.SubFacturasxCliente.Factura,
                                NumeroFel = recDet.SubFacturasxCliente.NumeroFEL,
                                FechaFactura = recDet.SubFacturasxCliente.FacturasxCliente.FechaFactura,
                                Tipo = rec.FacturasxCliente.Tipo,
                                ReciboId = recDet.ProformaId,
                                IdSubFactura = recDet.IdSubFactura,
                                Valor = recDet.Valor,
                                ValorFactura = recDet.SubFacturasxCliente.FacturasxCliente.TotalFactura,
                                ValorSinDescuento = (recDet.Valor ?? 0) + (recDet.Descuento ?? 0),
                                Descuento = recDet.Descuento,
                                EsAbono = recDet.EsAbono,
                                DiasVencimiento = DbFunctions.DiffDays(rec.Fecha, recDet.SubFacturasxCliente.FechaVencimiento) ?? 0
                            } : new RecibosDetalleViewModel
                            {
                                IdReciboDetalle = recDet.IdProformaDetalle,
                                Factura = "SALDO_FAVOR",
                                NumeroFel = "",
                                FechaFactura = null,
                                Tipo = "Pago",
                                ReciboId = recDet.IdProformaDetalle,
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
                    }
                        
                    return Ok(ListaRecibos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        public IHttpActionResult CrearProforma(ReciboPostViewModel proformaPost)
        {
            try
            {
                try
                {
                    var json = new JavaScriptSerializer().Serialize(proformaPost);
                    EscribirEnArchivo($"Recibo At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    RespuestaRecibo respuestaPagoRecibo = new RespuestaRecibo();
                    List<RecibosxClienteViewModel> recibosxCliente = new List<RecibosxClienteViewModel>();
                    List<RecibosxClienteFlotanteViewModel> recibosxClienteFlotante = new List<RecibosxClienteFlotanteViewModel>();
                    RecibosxClienteFlotanteViewModel reciboXClienteFlotante = new RecibosxClienteFlotanteViewModel();
                    proformaPost.FechaPago = new DateTime(proformaPost.FechaPago.Year, proformaPost.FechaPago.Month, proformaPost.FechaPago.Day);
                    var existeRecibo = 0;
                    var asesor = ctx.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                    var PagosBD = ctx.TiposdePago.AsNoTracking().ToList();
                    var BancosBD = ctx.Bancos.AsNoTracking().ToList();
                    List<ReciboApiModel> recibos = new List<ReciboApiModel>();
                    var codigoCliente = "";
                    int numeroCorrelativoRecibo = asesor.CorrelativoRecibos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    var subFacturas = ctx.SubFacturasxCliente.Include(b => b.FacturasxCliente).AsNoTracking().Where(subFac => proformaPost.SubFacturas.Contains(subFac.IdSubFactura)).OrderBy(subFac => subFac.FechaVencimiento).ToList();
                    foreach (PagosReciboPostViewModel pago in proformaPost.Pagos.OrderBy(pag => pag.Orden))
                    {
                        var pagoBD = PagosBD.FirstOrDefault(pa => pa.IdTipoPago.ToString() == pago.CodigoTipoPago);
                        var pagoDetalleBD = ctx.TiposdePagoDetalle.FirstOrDefault(pd => pd.IdTipoPago.ToString() == pago.CodigoTipoPago && pd.CodigoDetalle == pago.TipoPagoDetalle);
                        var respuestapago = new RespuestaPago
                        {
                            TipoPago = pagoBD.Descripcion,
                            EspecificacionPago = pagoDetalleBD.Descripcion,
                            Fecha = proformaPost.Fecha,
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
                            var Factura = ctx.FacturasxCliente.Where(fa => fa.Factura == subfactura.Factura).FirstOrDefault();
                            Factura.PendienteFactura = Decimal.Parse((valor).ToString());
                            if ((valor > 0) && (valorCuota > 0))
                            {
                                bool aplicaDescuento = false;
                                aplicaDescuento = ((subfactura.Descuento ?? 0) > 0 &&
                                    ((subfactura.FechaMaxDescuento.HasValue && proformaPost.FechaPago.Date <= subfactura.FechaMaxDescuento.Value.Date) ||
                                    (subfactura.FechaVencimientoDescuento.HasValue && proformaPost.FechaPago.Date <= subfactura.FechaVencimientoDescuento.Value.Date)
                                    ));
                                if (aplicaDescuento)
                                {
                                    valorCuota = Decimal.ToDouble(subfactura.Saldo.Value - subfactura.Descuento.Value);
                                }
                                var pagoValor = Decimal.Parse((valor).ToString());
                                var minutosConf = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotanteRecibo");
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
                                var recibo = recibosXPago.FirstOrDefault(rec => rec.TIPO_PAGO == pagoBD.Codigo && rec.REFERENCIA == pago.Referencia && rec.FACTURA == subfactura.Factura);
                                RecibosxClienteViewModel reciboXCliente = recibosxCliente.FirstOrDefault(recXCli => recXCli.IdTipoPago.ToString() == pago.CodigoTipoPago && recXCli.Referencia == pago.Referencia);
                                existeRecibo = ctx.RecibosProforma.Where(x => x.NumeroProforma == proformaPost.NumeroRecibo).Count();

                                if (existeRecibo == 0)
                                {
                                    existeRecibo = ctx.RecibosProforma.Where(x => x.CodigoCliente == subfactura.CodigoCliente
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
                                        RECIBO = proformaPost.NumeroRecibo,
                                        CLIENTE = subfactura.CodigoCliente,
                                        MONEDA = pago.IdMoneda,
                                        FECHA = DateTime.Now.ToString("dd/MM/yyyy"),
                                        DESCRIPCION = proformaPost.Descripcion,
                                        TIPO_PAGO = pagoBD.Codigo,
                                        SPEC_PAGO = pago.TipoPagoDetalle,
                                        BANCO = bank != null ? bank.NombreBanco : "",
                                        REFERENCIA = pago.Referencia,
                                        FECHA_PAGO = proformaPost.FechaPago.ToString("dd/MM/yyyy"),
                                        FACTURA = subfactura.Factura,
                                        APLICADO = "0",
                                        DESCUENTO = "0",
                                        REF_TRANSOPEN = subfactura.Referencia,
                                    };
                                    recibosXPago.Add(recibo);
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
                                        FechaPago = proformaPost.FechaPago,
                                        IdBanco = bank?.IdBanco,
                                        Valor = 0,
                                        IdMoneda = pago.IdMoneda,
                                        Sincronizado = false,
                                        CodigoAsesor = asesor.CodigoAsesor,
                                        IdFactura = subfactura.IdFactura,
                                        Descuento = 0,
                                        Latitude = (proformaPost.location != null) ? proformaPost.location.latitude : null,
                                        Longitude = (proformaPost.location != null) ? proformaPost.location.longitude : null,
                                        SpecPago = pago.TipoPagoDetalle,
                                        UsuarioCreacion = user.UserAccount,
                                        FechaCreacion = DateTime.Now,
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
                                        FechaPago = proformaPost.FechaPago,
                                        IdBanco = bank?.IdBanco,
                                        Valor = 0,
                                        IdMoneda = pago.IdMoneda,
                                        Sincronizado = false,
                                        CodigoAsesor = asesor.CodigoAsesor,
                                        IdFactura = subfactura.IdFactura,
                                        Descuento = 0,
                                        Latitude = (proformaPost.location != null) ? proformaPost.location.latitude : null,
                                        Longitude = (proformaPost.location != null) ? proformaPost.location.longitude : null,
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


                                var pagoAplicado = respuestaPagoRecibo.Facturas.FirstOrDefault(fact => fact.IdFactura == subfactura.Factura);
                                var fechaFactura = ctx.FacturasxCliente.FirstOrDefault(x => x.Factura == subfactura.Factura).FechaFactura;
                                if (pagoAplicado == null)
                                {
                                    TimeSpan ts = proformaPost.Fecha - subfactura.FechaVencimiento.Value;

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
                    if (proformaPost.SaldoFavor > 0)
                    {
                        RespuestaFactura pagoAplicado = new RespuestaFactura
                        {
                            IdFactura = "SALDO_FAVOR",
                            Fecha = DateTime.Today,
                            Dias = 0,
                            TipoDocumento = "Pago",
                            Aplicado = proformaPost.SaldoFavor,
                            Parcial = proformaPost.SaldoFavor,
                        };
                        respuestaPagoRecibo.Facturas.Add(pagoAplicado);
                        respuestaPagoRecibo.Total += proformaPost.SaldoFavor;
                        if (existeRecibo == 0)
                        {
                            var primerRecibo = recibosxCliente.FirstOrDefault();
                            primerRecibo.Valor += decimal.Parse(proformaPost.SaldoFavor.ToString());
                            primerRecibo.DetalleRecibo.Add(new RecibosDetalleViewModel
                            {
                                Valor = decimal.Parse(proformaPost.SaldoFavor.ToString()),
                                ValorSinDescuento = decimal.Parse(proformaPost.SaldoFavor.ToString())
                            });
                        }
                        else
                        {
                            var primerRecibo = recibosxClienteFlotante.FirstOrDefault();
                            primerRecibo.Valor += decimal.Parse(proformaPost.SaldoFavor.ToString());
                            primerRecibo.DetalleRecibo.Add(new RecibosDetalleViewModel
                            {
                                Valor = decimal.Parse(proformaPost.SaldoFavor.ToString()),
                                ValorSinDescuento = decimal.Parse(proformaPost.SaldoFavor.ToString())
                            });
                        }

                    }

                    if (existeRecibo == 0)
                    {
                        try
                        {
                            var Esduplicado = AsyncSqlInsert.IngresarRecibosProforma(recibosxCliente);

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
                                        Estado = 0,
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
                                var NumRecibo = recibosxCliente[0].NumeroRecibo;
                                var numProforma = ctx.RecibosProforma.Where(x=> x.NumeroProforma == NumRecibo).FirstOrDefault();
                                recibosxCliente[0].proformaId = numProforma.ProformaId;
                                var Caracteres= NumRecibo.Length;
                                var numRecibo = recibosxCliente[0].NumeroRecibo.Substring(2, Caracteres - 2);
                                recibosxCliente[0].NumeroRecibo = numRecibo;
                                AsyncSqlInsert.IngresarRecibos(recibosxCliente, false);

                                if (proformaPost.LogImpresion.Count() > 0)
                                {
                                    foreach(var logProforma in proformaPost.LogImpresion)
                                    {
                                        var LogProformas = new LogProforma()
                                        {
                                            ProformaId = numProforma.ProformaId,
                                            Usuario = logProforma.Usuario,
                                            Fecha = DateTime.Now,
                                            Latitude = logProforma.Latitude,
                                            Longitude = logProforma.longitude
                                        };
                                        ctx.LogProforma.Add(LogProformas);
                                        ctx.SaveChanges();
                                    }
                                }
                            }

                            respuestaPagoRecibo.Mensaje = "El recibo ha sido sincronizado exitosamente.";
                            return Ok(respuestaPagoRecibo);


                        }
                        catch (Exception e)
                        {
                            return BadRequest(e.ToString());
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
                        
                        respuestaPagoRecibo.Mensaje = "El recibo ha sido sincronizado exitosamente.";
                        return Ok(respuestaPagoRecibo);
                    }
                }
            }catch(Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}