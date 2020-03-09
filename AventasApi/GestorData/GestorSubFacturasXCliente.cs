using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;

namespace AventasApi.GestorData
{
    public class GestorSubFacturasXCliente
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/IMHN/gmonrroy/gmonrroy/0/{{0}}";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorSubFacturasXCliente()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<Clientes> clientes = new List<Clientes>();


                using (AVentasEntities context = new AVentasEntities())
                {
                    clientes = context.Clientes.ToList();
                }

                if (clientes != null && clientes.Count > 0)
                {
                    for (int i = 0; (i * 100) < clientes.Count(); i++)
                    {
                        List<Clientes> buffer = new List<Clientes>();
                        if ((i + 1) * 100 > clientes.Count())
                        {
                            buffer = clientes.GetRange(i * 100, clientes.Count() - (i * 100));

                        }
                        else
                        {
                            buffer = clientes.GetRange(i * 100, 100);


                        }
                        var taskGetacuerdos =
                            buffer.Select(async col =>
                        {
                            List<SubFacturasXClienteApiModel> facturasXCliente = new List<SubFacturasXClienteApiModel>();
                            //HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, "IMHN-000000272")).ConfigureAwait(false);
                            HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoCliente)).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                facturasXCliente = await response.Content.ReadAsAsync<List<SubFacturasXClienteApiModel>>();
                                facturasXCliente.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var factura =
                                            context.FacturasxCliente.FirstOrDefault(fac =>
                                                fac.Referencia == txg.REF_CUSTTRANS);
                                        try
                                        {


                                            SubFacturasxCliente acuerdo = new SubFacturasxCliente
                                            {
                                                EmpresaId = txg.ENTITY,
                                                CodigoCliente = txg.ACCOUNT_NUM,
                                                Factura = txg.INVOICE,
                                                Saldo = txg.AMOUNT_MST != null ? Convert.ToDecimal(txg.AMOUNT_MST) : 0,
                                                SaldoDivisa = txg.AMOUNT_CUR != null ? Convert.ToDecimal(txg.AMOUNT_CUR) : 0,
                                                FechaVencimiento = txg.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                FechaMaxDescuento = txg.LIMIT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.LIMIT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                FechaVencimientoDescuento = txg.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                Descuento = txg.DISC_AMOUNT != null ? Convert.ToDecimal(txg.DISC_AMOUNT) : 0,
                                                PendientePago = txg.PAYM_AMOUNT != null ? Convert.ToDecimal(txg.PAYM_AMOUNT) : 0,
                                                Referencia = txg.REF_TRANSOPEN,
                                                ReferenciaFacturas = txg.REF_CUSTTRANS,
                                                ReferenciaAcuerdo = txg.AGREEMENT_NUM,
                                                NumeroCuota = txg.PA_PAYM_NUM != null ? Convert.ToInt32(txg.PA_PAYM_NUM) : 0,
                                                ValorCuota = txg.PA_PAYM_AMOUNT != null ? Convert.ToDecimal(txg.PA_PAYM_AMOUNT) : 0,
                                                ValorVencidoCuota = txg.PA_DUE_AMOUNT != null ? Convert.ToDecimal(txg.PA_DUE_AMOUNT) : 0,
                                                ReferenciaCuotas = txg.PA_REF_APSA,
                                                IdMoneda = txg.CURRENCY_CODE,
                                                IdAcuerdoxCliente = txg.AGREEMENT_NAME == "" ? null : txg.AGREEMENT_NAME,
                                                IdFactura = factura.IdFactura
                                            };
                                            factura.IdAcuerdoxCliente = acuerdo.IdAcuerdoxCliente;
                                            context.SubFacturasxCliente.Add(acuerdo);
                                            context.SaveChanges();
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e);

                                        }

                                    }
                                });


                            }
                            else
                            {
                                Debug.WriteLine("Error en a peticion");

                            }

                        });
                        await Task.WhenAll(taskGetacuerdos);

                    }
                    Debug.WriteLine("FFinalizo");




                }


            });
        }
    }

}