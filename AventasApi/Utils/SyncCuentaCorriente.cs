using DBData.Database;
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
        private void UpdateFacturas(List<FacturasXClienteApiModel> facturas)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    _= context.SP_FacturasxCliente_UpdateSaldoXCliente(facturas[0].ACCOUNT_NUM, facturas[0].ENTITY);
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
        private void UpdateSubFacturas(List<SubFacturasXClienteApiModel> subFacturas)
        {
            try
            {
                using(AVentasEntities context = new AVentasEntities())
                {
                    _ = context.SP_SubFacturasxCliente_UpdateSaldoXCliente(subFacturas[0].ACCOUNT_NUM, subFacturas[0].ENTITY);
                    foreach (var subFactura in subFacturas)
                    {
                        var fFactura = context.FacturasxCliente.FirstOrDefault(x => x.Referencia == subFactura.REF_CUSTTRANS);
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
                          
                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                }
            }catch(Exception e)
            {

            }
        }
        public void SyncFacturas(string empresa,string codigoCliente)
        {
            try
            {
                var facturas = new List<FacturasXClienteApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"facturas/{empresa}/{codigoCliente}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    facturas = JsonConvert.DeserializeObject<List<FacturasXClienteApiModel>>(response.Content);
                }

                if(facturas.Count>0)
                {
                    UpdateFacturas(facturas);
                }

            }
            catch(Exception e)
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
                }

                if (facturas.Count > 0)
                {
                    UpdateSubFacturas(facturas);
                }

            }
            catch (Exception e)
            {

            }
        }
    }
}