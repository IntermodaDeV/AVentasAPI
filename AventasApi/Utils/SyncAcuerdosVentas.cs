using AventasApi.Models.Authentication;
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
    public class SyncAcuerdosVentas
    {
        public void SyncAcuerdoVenta(string empresa, string codigoCliente, string CodigoAsesor)
        {
            try
            {
                var acuerdos = new List<AcuerdoCRMApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"acuerdos/{empresa}/{CodigoAsesor}/{codigoCliente}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    acuerdos = JsonConvert.DeserializeObject<List<AcuerdoCRMApiModel>>(response.Content);
                }

                if (acuerdos.Count > 0)
                {
                    UpdateAcuerdosVentas(acuerdos);
                }

            }
            catch (Exception e)
            {
            }
        }

        public void SyncCuotasAcuerdoVenta(string acuerdoVenta)
        {
            try
            {
                var acuerdos = new List<CuotasXAcuerdoApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"acuerdos/cuotas/{acuerdoVenta}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    acuerdos = JsonConvert.DeserializeObject<List<CuotasXAcuerdoApiModel>>(response.Content);
                }

                if (acuerdos.Count > 0)
                {
                    UpdateCuotaAcuerdos(acuerdoVenta,acuerdos);
                }

            }
            catch (Exception e)
            {
            }
        }

        private void UpdateCuotaAcuerdos(string acuerdo,List<CuotasXAcuerdoApiModel> cuotas)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    foreach (var cuota in cuotas)
                    {
                        CuotasXAcuerdo model = new CuotasXAcuerdo
                        {
                            IdAcuerdoVenta = acuerdo,
                            NumCuota = cuota.IMPAYMENTNUMBER,
                            ValorCuota = cuota.AMOUNT,
                            SaldoDiponible = cuota.REMAINAMOUNT,
                            FechaVencimiento = cuota.DUEDATE
                        };

                        var entityFound = ctx.CuotasXAcuerdo.FirstOrDefault(p => p.IdAcuerdoVenta == model.IdAcuerdoVenta && p.NumCuota == model.NumCuota);

                        if (entityFound == null)
                        {
                            ctx.CuotasXAcuerdo.Add(model);
                        }
                        else
                        {
                            entityFound.NumCuota = model.NumCuota;
                            entityFound.ValorCuota = model.ValorCuota;
                            entityFound.SaldoDiponible = model.SaldoDiponible;
                            entityFound.FechaVencimiento = model.FechaVencimiento;
                            ctx.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }


        private void UpdateAcuerdosVentas(List<AcuerdoCRMApiModel> acuerdos)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    foreach (var acuerdo in acuerdos)
                    {
                        var entityFound = context.AcuerdosxCliente.FirstOrDefault(x => x.IdAcuerdoxCliente == acuerdo.ID_SALES_AGREEMENT && x.CodigoCliente == acuerdo.CUSTOMER_ACCOUNT);
                      

                        if (entityFound == null)
                        {
                            var newEntity = new AcuerdosxCliente();
                            DateTime dummy = new DateTime();
                            decimal tAcuerdo = 0, sAcuerdo = 0, EnPedido = 0, Facturado = 0, Entregado;
                            newEntity.IdAcuerdoxCliente = acuerdo.ID_SALES_AGREEMENT;
                            newEntity.CodigoCliente = acuerdo.CUSTOMER_ACCOUNT;
                            newEntity.IdTipoPedido = context.TiposdePedido.FirstOrDefault(p => p.TipoPedido == acuerdo.CLASS_SALES_AGREEMENT).IdTipoPedido;
                            newEntity.IdMoneda = acuerdo.CURRENCY;
                            newEntity.EmpresaId = acuerdo.ENTITY;
                            newEntity.Total = Decimal.TryParse(acuerdo.AMOUNT, out tAcuerdo) ? tAcuerdo : 0;
                            newEntity.Saldo = Decimal.TryParse(acuerdo.REMAINING, out sAcuerdo) ? sAcuerdo : 0;
                            newEntity.Liberado = Decimal.TryParse(acuerdo.RELEASED, out EnPedido) ? EnPedido : 0;
                            newEntity.Facturado = Decimal.TryParse(acuerdo.INVOICED, out Facturado) ? Facturado : 0;
                            newEntity.Entregado = Decimal.TryParse(acuerdo.DELIVERED, out Entregado) ? Entregado : 0;
                            newEntity.IdLinea = acuerdo.LINE;
                            newEntity.Desde = DateTime.TryParseExact(acuerdo.STARTDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(acuerdo.STARTDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now; 
                            newEntity.Hasta = DateTime.TryParseExact(acuerdo.ENDDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(acuerdo.ENDDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now; ;
                            context.AcuerdosxCliente.Add(newEntity);
                        }
                        else
                        {
                            DateTime dummy = new DateTime();
                            decimal tAcuerdo = 0, sAcuerdo = 0, EnPedido = 0, Facturado = 0, Entregado;
                            entityFound.IdAcuerdoxCliente = acuerdo.ID_SALES_AGREEMENT;
                            entityFound.CodigoCliente = acuerdo.CUSTOMER_ACCOUNT;
                            entityFound.IdTipoPedido = context.TiposdePedido.FirstOrDefault(p => p.TipoPedido == acuerdo.CLASS_SALES_AGREEMENT).IdTipoPedido;
                            entityFound.IdMoneda = acuerdo.CURRENCY;
                            entityFound.EmpresaId = acuerdo.ENTITY;
                            entityFound.Total = Decimal.TryParse(acuerdo.AMOUNT, out tAcuerdo) ? tAcuerdo : 0;
                            entityFound.Saldo = Decimal.TryParse(acuerdo.REMAINING, out sAcuerdo) ? sAcuerdo : 0;
                            entityFound.Liberado = Decimal.TryParse(acuerdo.RELEASED, out EnPedido) ? EnPedido : 0;
                            entityFound.Facturado = Decimal.TryParse(acuerdo.INVOICED, out Facturado) ? Facturado : 0;
                            entityFound.Entregado = Decimal.TryParse(acuerdo.DELIVERED, out Entregado) ? Entregado : 0;
                            entityFound.IdLinea = acuerdo.LINE;
                            entityFound.Desde = DateTime.TryParseExact(acuerdo.STARTDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(acuerdo.STARTDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                            entityFound.Hasta = DateTime.TryParseExact(acuerdo.ENDDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy) ? DateTime.ParseExact(acuerdo.ENDDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now; ;

                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}