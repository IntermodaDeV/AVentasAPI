using DBData.Database;
using ExternalApiData.ApiModels;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AventasApi.Utils
{
    public class SyncCuentaCorriente
    {
        private void UpdateFacturas(string codigoCliente,string empresa,List<FacturasXClienteApiModel> facturas)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                   context.SP_FacturasxCliente_UpdateSaldoXCliente(codigoCliente, empresa);
                   foreach(var factura in facturas)
                    {
                        var entityFound = context.FacturasxCliente.FirstOrDefault(x => x.Factura == factura.INVOICE);
                        var tipoPedido  = context.TiposdePedido.FirstOrDefault(x => x.TipoPedido == factura.DOC_TYPE);
                        int a = 0;

                        if(entityFound == null)
                        {
                            var newEntity = new FacturasxCliente();
                            DateTime dummy = new DateTime();
                            decimal tFactura = 0, sFactura = 0, pFactura = 0, desc = 0;
                            int nPagos = 0;
                            newEntity.Factura = factura.INVOICE;
                            newEntity.Clientes = context.Clientes.FirstOrDefault(p => p.CodigoCliente == factura.ACCOUNT_NUM);
                            newEntity.TiposdePedido = context.TiposdePedido.FirstOrDefault(p => p.IdTipoPedido == tipoPedido.IdTipoPedido);
                            newEntity.MaestroMoneda = context.MaestroMoneda.FirstOrDefault(p => p.IdMoneda == factura.CURRENCY_CODE);
                            newEntity.Empresa = context.Empresa.FirstOrDefault(p => p.EmpresaId == factura.ENTITY);
                            newEntity.MaestroLinea = context.MaestroLinea.FirstOrDefault(p => p.IdLinea == factura.PROD_LINE);
                            newEntity.AcuerdosxCliente =null;
                            newEntity.Tipo = factura.TRANS_TYPE;
                            newEntity.FechaFactura = DateTime.TryParseExact(factura.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.FechaVencimiento = DateTime.TryParseExact(factura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.FechaMaxDescuento = DateTime.TryParseExact(factura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.TotalFactura = Decimal.TryParse(factura.AMOUNT_CUR, out tFactura) ? tFactura : 0;
                            newEntity.PendienteFactura = Decimal.TryParse(factura.AMOUNT_PENDING, out pFactura) ? pFactura : 0;
                            newEntity.Saldo = Decimal.TryParse(factura.REMAIN_AMOUNT_CUR, out sFactura) ? sFactura : 0;
                            newEntity.Descuento = Decimal.TryParse(factura.DISCOUNT, out desc) ? desc : 0;         
                            /*if (newEntity.PendienteFactura > 0)
                            {
                                newEntity.Saldo = newEntity.Saldo - newEntity.PendienteFactura - (entityFound.FechaMaxDescuento < DateTime.Today ? 0 : entityFound.Descuento);
                            }*/
                            //newEntity.NumeroPedido = factura.SALESID;
                            newEntity.FacturaStatus = factura.STATUS;
                            newEntity.NumeroPagos = int.TryParse(factura.N_PAYMENTS, out nPagos) ? nPagos : 0;
                            newEntity.Referencia = factura.REF_TRANS;
                            newEntity.DiasGracia = 0;
                            newEntity.CodigoDescuento = factura.KREACASHDISCCODE;
                            context.FacturasxCliente.Add(newEntity);
                        }
                        else
                        {
                            DateTime dummy = new DateTime();
                            decimal tFactura = 0, sFactura = 0, pFactura = 0, desc = 0;
                            int nPagos = 0;
                            entityFound.Clientes = context.Clientes.FirstOrDefault(p => p.CodigoCliente == factura.ACCOUNT_NUM);
                            entityFound.TiposdePedido = context.TiposdePedido.FirstOrDefault(p => p.IdTipoPedido == tipoPedido.IdTipoPedido);
                            entityFound.MaestroMoneda = context.MaestroMoneda.FirstOrDefault(p => p.IdMoneda == factura.CURRENCY_CODE);
                            entityFound.Empresa = context.Empresa.FirstOrDefault(p => p.EmpresaId == factura.ENTITY);
                            entityFound.MaestroLinea = context.MaestroLinea.FirstOrDefault(p => p.IdLinea == factura.PROD_LINE);
                            entityFound.AcuerdosxCliente = null;
                            entityFound.Tipo = factura.TRANS_TYPE;
                            entityFound.FechaFactura = DateTime.TryParseExact(factura.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.FechaVencimiento = DateTime.TryParseExact(factura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now; 
                            entityFound.FechaMaxDescuento = DateTime.TryParseExact(factura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(factura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.TotalFactura = Decimal.TryParse(factura.AMOUNT_CUR, out tFactura) ? tFactura : 0;
                            entityFound.Saldo = Decimal.TryParse(factura.REMAIN_AMOUNT_CUR, out sFactura) ? sFactura : 0;
                            entityFound.PendienteFactura = Decimal.TryParse(factura.AMOUNT_PENDING, out pFactura) ? pFactura : 0;
                            entityFound.Descuento = Decimal.TryParse(factura.DISCOUNT, out desc) ? desc : 0;

                            if (entityFound.PendienteFactura > 0)
                            {
                                entityFound.Saldo = entityFound.Saldo - entityFound.PendienteFactura - (entityFound.FechaMaxDescuento < DateTime.Today ? 0 : entityFound.Descuento);
                            }
                           
                            entityFound.FacturaStatus = factura.STATUS;
                            entityFound.NumeroPagos = int.TryParse(factura.N_PAYMENTS, out nPagos) ? nPagos : 0;
                            entityFound.Referencia = factura.REF_TRANS;
                            entityFound.CodigoDescuento = factura.KREACASHDISCCODE;

                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch(Exception e)
            {

            }
        }
        private void UpdateSubFacturas(string codigoCliente,string empresa,List<SubFacturasXClienteApiModel> subFacturas)
        {
            try
            {
                using(AVentasEntities context = new AVentasEntities())
                {
                    context.SP_SubFacturasxCliente_UpdateSaldoXCliente(codigoCliente, empresa);
                    foreach (var subFactura in subFacturas)
                    {
                        var fFactura = context.FacturasxCliente.FirstOrDefault(x => x.Referencia == subFactura.REF_CUSTTRANS && x.Factura.ToUpper() == subFactura.INVOICE.ToUpper());

                        if (fFactura == null)
                        {
                            continue;
                        }

                        var entityFound = context.SubFacturasxCliente.FirstOrDefault(p => p.Factura == fFactura.Factura && p.Referencia == subFactura.REF_TRANSOPEN);

                        if (entityFound == null)
                        {
                            var newEntity = new SubFacturasxCliente();
                            decimal tFacturaDivisa, ssFactura = 0, psFactura = 0, sDesc = 0, svCuota = 0, svVencidoCuota = 0;
                            int snCuota = 0;
                            DateTime dummy = new DateTime();

                            newEntity.Factura = fFactura.Factura;
                            newEntity.FacturasxCliente = context.FacturasxCliente.FirstOrDefault(p => p.Factura == fFactura.Factura);
                            newEntity.AcuerdosxCliente = context.AcuerdosxCliente.FirstOrDefault(x => x.IdAcuerdoxCliente == subFactura.AGREEMENT_NAME);
                            newEntity.Clientes = context.Clientes.FirstOrDefault(p => p.CodigoCliente == subFactura.ACCOUNT_NUM);
                            newEntity.Empresa = context.Empresa.FirstOrDefault(p => p.EmpresaId == subFactura.ENTITY);
                            newEntity.MaestroMoneda = context.MaestroMoneda.FirstOrDefault(p => p.IdMoneda == subFactura.CURRENCY_CODE);
                            newEntity.AcuerdosxCliente = context.AcuerdosxCliente.FirstOrDefault(p => p.IdAcuerdoxCliente == subFactura.AGREEMENT_NAME);
                            newEntity.FechaVencimiento = DateTime.TryParseExact(subFactura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.FechaMaxDescuento = DateTime.TryParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.FechaVencimientoDescuento = DateTime.TryParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            newEntity.Saldo = Decimal.TryParse(subFactura.AMOUNT_CUR, out ssFactura) ? ssFactura : 0;
                            fFactura.IdAcuerdoxCliente = String.IsNullOrEmpty(subFactura.AGREEMENT_NAME) ? null : subFactura.AGREEMENT_NAME;
                            if (fFactura.Saldo == 0)
                            {
                                newEntity.Saldo = 0;
                            }

                            newEntity.SaldoDivisa = Decimal.TryParse(subFactura.AMOUNT_MST, out tFacturaDivisa) ? tFacturaDivisa : 0;
                            newEntity.Descuento = Decimal.TryParse(subFactura.DISC_AMOUNT, out sDesc) ? sDesc : 0;
                            newEntity.PendientePago = Decimal.TryParse(subFactura.PAYM_AMOUNT, out psFactura) ? psFactura : 0;
                            newEntity.Referencia = subFactura.REF_TRANSOPEN;
                            newEntity.ReferenciaFacturas = subFactura.REF_CUSTTRANS;
                            newEntity.ReferenciaAcuerdo = subFactura.AGREEMENT_NUM;
                            newEntity.NumeroCuota = int.TryParse(subFactura.PA_PAYM_NUM, out snCuota) ? snCuota : 0;
                            newEntity.ValorCuota = Decimal.TryParse(subFactura.PA_PAYM_AMOUNT, out svCuota) ? svCuota : 0;
                            newEntity.ValorVencidoCuota = Decimal.TryParse(subFactura.PA_DUE_AMOUNT, out svVencidoCuota) ? svVencidoCuota : 0;
                            newEntity.ReferenciaCuotas = subFactura.PA_REF_APSA;
                            newEntity.Valor = !String.IsNullOrEmpty(subFactura.AGREEMENT_NAME) ? Decimal.TryParse(subFactura.PA_DUE_AMOUNT, out ssFactura) ? ssFactura : 0 : fFactura.TotalFactura;
                            newEntity.Flete = Decimal.TryParse(subFactura.FREIGHT, out ssFactura) ? ssFactura : 0;
                            newEntity.completaCuota = false;           

                            context.SubFacturasxCliente.Add(newEntity);
                        }
                        else
                        {
                            decimal tFacturaDivisa, ssFactura = 0, psFactura = 0, sDesc = 0, svCuota = 0, svVencidoCuota = 0;
                            int snCuota = 0;
                            DateTime dummy = new DateTime();
                            entityFound.Factura = fFactura.Factura;
                            entityFound.FacturasxCliente = context.FacturasxCliente.FirstOrDefault(p => p.Factura == fFactura.Factura);
                            entityFound.Clientes = context.Clientes.FirstOrDefault(p => p.CodigoCliente == subFactura.ACCOUNT_NUM);
                            entityFound.Empresa = context.Empresa.FirstOrDefault(p => p.EmpresaId == subFactura.ENTITY);
                            entityFound.MaestroMoneda = context.MaestroMoneda.FirstOrDefault(p => p.IdMoneda == subFactura.CURRENCY_CODE);
                            entityFound.AcuerdosxCliente = context.AcuerdosxCliente.FirstOrDefault(p => p.IdAcuerdoxCliente == subFactura.AGREEMENT_NAME);
                            entityFound.FechaVencimiento = DateTime.TryParseExact(subFactura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.FechaMaxDescuento = DateTime.TryParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.FechaVencimientoDescuento = DateTime.TryParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(subFactura.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.Saldo = Decimal.TryParse(subFactura.AMOUNT_CUR, out ssFactura) ? ssFactura : 0;
                            if (fFactura.Saldo == 0)
                            {
                                entityFound.Saldo = 0;
                            }
                            entityFound.IdAcuerdoxCliente = String.IsNullOrEmpty(subFactura.AGREEMENT_NAME) ? null: subFactura.AGREEMENT_NAME;
                            fFactura.IdAcuerdoxCliente = entityFound.IdAcuerdoxCliente;
                            entityFound.SaldoDivisa = Decimal.TryParse(subFactura.AMOUNT_MST, out tFacturaDivisa) ? tFacturaDivisa : 0;
                            entityFound.Descuento = Decimal.TryParse(subFactura.DISC_AMOUNT, out sDesc) ? sDesc : 0;
                            entityFound.PendientePago = Decimal.TryParse(subFactura.PAYM_AMOUNT, out psFactura) ? psFactura : 0;
                            entityFound.Referencia = subFactura.REF_TRANSOPEN;
                            entityFound.ReferenciaFacturas = subFactura.REF_CUSTTRANS;
                            entityFound.ReferenciaAcuerdo = subFactura.AGREEMENT_NUM;
                            entityFound.NumeroCuota = int.TryParse(subFactura.PA_PAYM_NUM, out snCuota) ? snCuota : 0;
                            entityFound.ValorCuota = Decimal.TryParse(subFactura.PA_PAYM_AMOUNT, out svCuota) ? svCuota : 0;
                            entityFound.ValorVencidoCuota = Decimal.TryParse(subFactura.PA_DUE_AMOUNT, out svVencidoCuota) ? svVencidoCuota : 0;
                            entityFound.ReferenciaCuotas = subFactura.PA_REF_APSA;
                            entityFound.Flete = Decimal.TryParse(subFactura.FREIGHT, out ssFactura) ? ssFactura : 0;
                            entityFound.completaCuota = false;
                            entityFound.Valor = !String.IsNullOrEmpty(subFactura.AGREEMENT_NAME) ? Decimal.TryParse(subFactura.PA_DUE_AMOUNT, out ssFactura) ? ssFactura : 0 : fFactura.TotalFactura;

                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                }
            }catch(Exception e)
            {

            }
        }
        private void UpdateDocumentosAplicados(string empresa, List<DocumentosAplicadosFacturaApiModel> documentos)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                { 
                    foreach(var datos in documentos)
                    {
                        DocumentosAplicadosAFacturas model = new DocumentosAplicadosAFacturas
                        {
                            Factura = datos.INVOICEORIGIN,
                            Voucher = datos.OFFSETTRANSVOUCHER,
                            TipoDocumento = datos.TAXTYPEDOCUMENTID,
                            FacturaDocumento = datos.INVOICEID,
                            Valor = decimal.Round(Convert.ToDecimal(datos.SETTLEAMOUNTCUR), 2),
                            MontoPorAplicar = decimal.Round(Convert.ToDecimal(datos.SETTLEAMOUNTCUR), 2),
                            CodigoCliente = datos.ACCOUNTNUM,
                            SecuenciaNumerica = datos.NUMBERSEQUENCEGROUP,
                            Moneda = datos.CURRENCYCODE,
                            Empresa = empresa,
                            FechaCrea = DateTime.Now
                        };

                        var entityFound = context.DocumentosAplicadosAFacturas.FirstOrDefault(p => p.Empresa == model.Empresa && p.CodigoCliente == model.CodigoCliente && p.Factura == model.Factura && p.Voucher == model.Voucher && p.Valor == model.Valor && p.SecuenciaNumerica == model.SecuenciaNumerica);
                        if(entityFound == null)
                        {
                            context.DocumentosAplicadosAFacturas.Add(model);
                        }
                        else
                        {
                            entityFound.Factura = model.Factura;
                            entityFound.Voucher = model.Voucher;
                            entityFound.TipoDocumento = model.TipoDocumento;
                            entityFound.FacturaDocumento = model.FacturaDocumento;
                            entityFound.Valor = model.Valor;
                            entityFound.CodigoCliente = model.CodigoCliente;
                            entityFound.SecuenciaNumerica = model.SecuenciaNumerica;
                            entityFound.Moneda = model.Moneda;
                            entityFound.Empresa = model.Empresa;
                            entityFound.SubFacturasxCliente = context.SubFacturasxCliente.FirstOrDefault(p => p.Empresa.EmpresaId == model.Empresa && p.Clientes.CodigoCliente == model.CodigoCliente && p.Factura == model.Factura);
                        }

                        context.SaveChanges();
                    }
                }
            }catch(Exception e)
            {

            }
        }
        private void UpdateDocumentosEnTransito(string asesor, List<DocumentonsEnTransitoApiModel> documentos)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.SP_DocumentosTransitoxFactura_UpdateSaldo(asesor);
                    context.SP_Devoluciones_UpdateEstado(asesor);

                    foreach(var datos in documentos)
                    {
                        DateTime dummy = new DateTime();

                       
                        string factura = null;
                        decimal valor = 0;
                        int tableId = 0;
                        int? idSubFactura = null;
                        var fFactura = context.FacturasxCliente.FirstOrDefault(x => x.Referencia == datos.REF_TRANS && x.Factura == datos.INVOICE && x.EmpresaId == datos.ENTITY.ToUpper());
                        if (fFactura != null)
                        {
                            factura = fFactura.Factura;
                        }

                        var sFactura = context.SubFacturasxCliente.FirstOrDefault(x => x.Referencia == datos.REF_TRANSOPEN && x.Factura == datos.INVOICE && x.CodigoCliente == datos.ACCOUNTNUM && x.EmpresaId == datos.ENTITY.ToUpper());
                        if (sFactura != null)
                        {
                            idSubFactura = sFactura.IdSubFactura;
                        }

                        DocumentosTransitoxFactura model = new DocumentosTransitoxFactura
                        {
                            Factura = factura,
                            CreadoPor = datos.CREATEDBY,
                            Estado = datos.DOCUMENT_STATUS,
                            FechaCreacion = DateTime.TryParseExact(datos.CREATEDDATETIME, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(datos.CREATEDDATETIME, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now,
                            IdSubFactura = idSubFactura,
                            NumeroDocumento = datos.DOCUMENT_NUMBER,
                            TablaId = int.TryParse(datos.SPECTABLEID, out tableId) ? tableId : 0,
                            Tipo = datos.TYPE,
                            Valor = Decimal.TryParse(datos.AMOUNT, out valor) ? valor : 0,
                            CodigoCliente = datos.ACCOUNTNUM,
                            EmpresaId = datos.ENTITY.ToUpper(),
                            IdMoneda = datos.CURRENCY,
                            Referencia = datos.REF_SPECTRANS,
                            ReferenciaFacturas = datos.REF_TRANS,
                            ReferenciaSubFactura = datos.REF_TRANSOPEN,
                            NumeroFEL = datos.FACTURACION_FEL
                        };

                        var entityFound = context.DocumentosTransitoxFactura.FirstOrDefault(p => p.Empresa.EmpresaId == model.EmpresaId && p.Clientes.CodigoCliente == model.CodigoCliente && p.SubFacturasxCliente.IdSubFactura == model.IdSubFactura && p.FacturasxCliente.Factura == model.Factura && p.Referencia == model.Referencia);
                        if (entityFound == null)
                        {
                            context.DocumentosTransitoxFactura.Add(model);
                        }
                        else
                        {
                            entityFound.Factura = model.Factura;
                            entityFound.NumeroFEL = model.NumeroFEL;
                            entityFound.Referencia = model.Referencia;
                            entityFound.ReferenciaFacturas = model.ReferenciaFacturas;
                            entityFound.CreadoPor = model.CreadoPor;
                            entityFound.Estado = model.Estado;
                            entityFound.FechaCreacion = model.FechaCreacion;
                            entityFound.NumeroDocumento = model.NumeroDocumento;
                            entityFound.TablaId = model.TablaId;
                            entityFound.Tipo = model.Tipo;
                            entityFound.Valor = model.Valor;
                            entityFound.ReferenciaSubFactura = model.ReferenciaSubFactura;
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void UpdateFleteSubfacturasAcuerdo(string codigoCliente)
        {
            try
            {
                using(var context = new AVentasEntities())
                {
                    var subFacturas = context.IMObtenerFacturasFletes(codigoCliente).ToList();
                    if(subFacturas.Count > 0)
                    {
                        foreach(var sub in subFacturas)
                        {
                            var subFacturasBD = context.SubFacturasxCliente.Where(x => x.Factura == sub.FACTURA.ToUpper() && x.CodigoCliente.ToUpper() == codigoCliente.ToUpper() && x.Saldo > 0).ToList();

                            if(subFacturasBD.Count > 0)
                            {
                                var fleteMaximo = subFacturasBD.Max(x => x.Flete);
                                var flete = fleteMaximo / sub.CANTIDAD;
                                foreach (var subf in subFacturasBD)
                                {
                                    subf.Flete = flete;
                                    context.SaveChanges();
                                }
                            } 
                        }
                    }
                }
            }catch(Exception e)
            {

            }
        }
        private void UpdateFacturasCuotaCero()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    ctx.SPActualizarSubFacturasCuotaCero();

                    var configuracion = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "VencimientoCuotaCero");
                    if (configuracion == null)
                    {
                        return;
                    }

                    var cuotas = ctx.SPObtenerSaldoCuotaCero().ToList();

                    foreach(var cuota in cuotas)
                    {

                        var entityFound = ctx.CuotasXAcuerdo.FirstOrDefault(x => x.IdAcuerdoVenta == cuota.IdAcuerdoxCliente && x.NumCuota == cuota.NumeroCuota);
                        var fechaVencimientoCuota = DateTime.ParseExact(configuracion.Valor, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        if (entityFound == null)
                        {
                            var cuotaBD = new CuotasXAcuerdo() { IdAcuerdoVenta = cuota.IdAcuerdoxCliente, FechaVencimiento = fechaVencimientoCuota, SaldoDiponible = 0, ValorCuota = cuota.Saldo ?? 0, NumCuota = 0 };
                            ctx.CuotasXAcuerdo.Add(cuotaBD);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            if (entityFound.ValorCuota >= (cuota.Saldo ?? 0))
                            {
                                entityFound.ValorCuota = cuota.Saldo ?? 0;
                                ctx.SaveChanges();
                            }
                        }

                    }
                }
            }catch(Exception e)
            {

            }
        }

        public void SyncFacturas(string empresa,string codigoCliente,string codigoasesor)
        {
            try
            {
                var facturas = new List<FacturasXClienteApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"facturas/{empresa}/1/{codigoasesor}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    facturas = JsonConvert.DeserializeObject<List<FacturasXClienteApiModel>>(response.Content);
                    var facturascliente = facturas.Where(x => x.ACCOUNT_NUM == codigoCliente).ToList();
                    UpdateFacturas(codigoCliente, empresa, facturascliente);
                }

            }
            catch (Exception e)
            {

            }
        }
        public void SyncSubFacturas(string empresa, string codigoCliente,string asesor)
        {
            try
            {
                var facturas = new List<SubFacturasXClienteApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"facturas/{empresa}/{asesor}/{asesor}/0/{codigoCliente}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    facturas = JsonConvert.DeserializeObject<List<SubFacturasXClienteApiModel>>(response.Content);
                    UpdateSubFacturas(codigoCliente, empresa, facturas);

                    if (facturas.Count() > 0)
                    {
                        UpdateFacturasCuotaCero();
                        UpdateFleteSubfacturasAcuerdo(codigoCliente);
                    }
                }

            }
            catch (Exception e)
            {

            }
        }
        public void SyncDocumentosAplicadosFactura(string empresa,string asesor)
        {
            try
            {
                var facturas = new List<DocumentosAplicadosFacturaApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"facturas/{empresa}/{asesor}/DocumentosAplicadosFactura", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    facturas = JsonConvert.DeserializeObject<List<DocumentosAplicadosFacturaApiModel>>(response.Content);
                    UpdateDocumentosAplicados(empresa, facturas);
                }
            }
            catch(Exception e)
            {

            }
        }
        public void SyncDocumentosEnTransito(string empresa, string asesor)
        {
            try
            {
                var docs = new List<DocumentonsEnTransitoApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"facturas/{empresa}/{asesor}/documentosentransito", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    docs = JsonConvert.DeserializeObject<List<DocumentonsEnTransitoApiModel>>(response.Content);

                    if (docs.Count() > 0)
                    {
                        UpdateDocumentosEnTransito(asesor, docs);
                    }
                }
                
            }
            catch(Exception e) { }
        }
    }
}