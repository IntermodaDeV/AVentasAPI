using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using DBData.Database;
using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;

namespace AventasApi.GestorData
{
    public class GestorFacturasXCliente
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/imhn/1/{{0}}/{{1}}/FactCliente";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorFacturasXCliente()
        {
            //ReiniciarTaskActualizarLineas();
            ReiniciarTaskActualizarLineas("hbenitez", "IMHN-000000564");

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                //List<Clientes> clientes = new List<Clientes>();
                var clientes = new List<AsesorXClienteViewModel>();

                using (AVentasEntities context = new AVentasEntities())
                {
                    //clientes = context.Clientes.ToList();
                    clientes = context.RutasxAsesor.SelectMany(rutAse => rutAse.Rutas.ClientesxRuta).Select(cliRut => new AsesorXClienteViewModel
                    {
                        CodigoAsesor = cliRut.Rutas.RutasxAsesor.FirstOrDefault().Asesores.Usuario,
                        ClienteId = cliRut.CodigoCliente
                    }).ToList();
                }
                //clientes.ForEach(cli => { 
                //    Debug.WriteLine(cli.ClienteId);
                //});
                //Debug.WriteLine("cantidad original");

                //Debug.WriteLine(clientes.Count);
                if (clientes != null && clientes.Count > 0)
                {

                    for (int i = 0; (i * 100) < clientes.Count(); i++)
                    {
                        List<AsesorXClienteViewModel> buffer = new List<AsesorXClienteViewModel>();
                        if ((i + 1) * 100 > clientes.Count())
                        {
                            buffer = clientes.GetRange(i * 100, clientes.Count() - ((i) * 100));
                        }
                        else
                        {
                            buffer = clientes.GetRange(i * 100, 100);
                        }
                        var taskGetacuerdos =
                        buffer.Select(async col =>
                        {
                            List<FacturasXClienteApiModel> facturasXCliente = new List<FacturasXClienteApiModel>();
                            HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoAsesor, col.ClienteId)).ConfigureAwait(false);
                            //HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoAsesor, col.ClienteId)).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                facturasXCliente = await response.Content.ReadAsAsync<List<FacturasXClienteApiModel>>();
                                facturasXCliente.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        try
                                        {
                                            var tipoPedido =
                                                context.TiposdePedido.FirstOrDefault(tp =>
                                                    tp.TipoPedido == txg.DOC_TYPE);

                                            FacturasxCliente acuerdo = new FacturasxCliente
                                            {
                                                EmpresaId = txg.ENTITY,
                                                CodigoCliente = txg.ACCOUNT_NUM,
                                                Factura = txg.INVOICE,
                                                Tipo = txg.TRANS_TYPE,
                                                FechaFactura = txg.DOCUMENT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                TotalFactura = txg.AMOUNT_CUR != null ? Convert.ToDecimal(txg.AMOUNT_CUR) : 0,
                                                Saldo = txg.REMAIN_AMOUNT_CUR != null ? Convert.ToDecimal(txg.REMAIN_AMOUNT_CUR) : 0,
                                                PendienteFactura = txg.AMOUNT_PENDING != null ? Convert.ToDecimal(txg.AMOUNT_PENDING) : 0,
                                                FechaVencimiento = txg.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                FechaMaxDescuento = txg.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                Descuento = txg.DISCOUNT != null ? Convert.ToDecimal(txg.DISCOUNT) : 0,
                                                IdMoneda = txg.CURRENCY_CODE,
                                                FacturaStatus = txg.STATUS,
                                                NumeroPagos = txg.N_PAYMENTS != null ? Convert.ToInt32(txg.N_PAYMENTS) : 0,
                                                Referencia = txg.REF_TRANS,
                                                IdLinea = txg.PROD_LINE == "" ? null : txg.PROD_LINE,
                                                IdTipoPedido = tipoPedido?.IdTipoPedido,
                                            };
                                            context.FacturasxCliente.Add(acuerdo);
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
        public static async void ActualizarFacturas()
        {
            await Task.Delay(20 * 1000);
            Debug.WriteLine("Finalizo");
            return;
                //List<Clientes> clientes = new List<Clientes>();
                var clientes = new List<AsesorXClienteViewModel>();

                using (AVentasEntities context = new AVentasEntities())
                {
                    //clientes = context.Clientes.ToList();
                    clientes = context.RutasxAsesor.SelectMany(rutAse => rutAse.Rutas.ClientesxRuta).Select(cliRut => new AsesorXClienteViewModel
                    {
                        CodigoAsesor = cliRut.Rutas.RutasxAsesor.FirstOrDefault().Asesores.Usuario,
                        ClienteId = cliRut.CodigoCliente
                    }).ToList();
                }
                //clientes.ForEach(cli => { 
                //    Debug.WriteLine(cli.ClienteId);
                //});
                //Debug.WriteLine("cantidad original");

                //Debug.WriteLine(clientes.Count);
                if (clientes != null && clientes.Count > 0)
                {

                    for (int i = 0; (i * 100) < clientes.Count(); i++)
                    {
                        List<AsesorXClienteViewModel> buffer = new List<AsesorXClienteViewModel>();
                        if ((i + 1) * 100 > clientes.Count())
                        {
                            buffer = clientes.GetRange(i * 100, clientes.Count() - ((i) * 100));
                        }
                        else
                        {
                            buffer = clientes.GetRange(i * 100, 100);
                        }
                        var taskGetacuerdos =
                        buffer.Select(async col =>
                        {
                            List<FacturasXClienteApiModel> facturasXCliente = new List<FacturasXClienteApiModel>();
                            HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoAsesor, col.ClienteId)).ConfigureAwait(false);
                            //HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, col.CodigoAsesor, col.ClienteId)).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                facturasXCliente = await response.Content.ReadAsAsync<List<FacturasXClienteApiModel>>();
                                facturasXCliente.ForEach(txg =>
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        try
                                        {
                                            var tipoPedido =
                                                context.TiposdePedido.FirstOrDefault(tp =>
                                                    tp.TipoPedido == txg.DOC_TYPE);

                                            FacturasxCliente acuerdo = new FacturasxCliente
                                            {
                                                EmpresaId = txg.ENTITY,
                                                CodigoCliente = txg.ACCOUNT_NUM,
                                                Factura = txg.INVOICE,
                                                Tipo = txg.TRANS_TYPE,
                                                FechaFactura = txg.DOCUMENT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                TotalFactura = txg.AMOUNT_CUR != null ? Convert.ToDecimal(txg.AMOUNT_CUR) : 0,
                                                Saldo = txg.REMAIN_AMOUNT_CUR != null ? Convert.ToDecimal(txg.REMAIN_AMOUNT_CUR) : 0,
                                                PendienteFactura = txg.AMOUNT_PENDING != null ? Convert.ToDecimal(txg.AMOUNT_PENDING) : 0,
                                                FechaVencimiento = txg.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                FechaMaxDescuento = txg.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                Descuento = txg.DISCOUNT != null ? Convert.ToDecimal(txg.DISCOUNT) : 0,
                                                IdMoneda = txg.CURRENCY_CODE,
                                                FacturaStatus = txg.STATUS,
                                                NumeroPagos = txg.N_PAYMENTS != null ? Convert.ToInt32(txg.N_PAYMENTS) : 0,
                                                Referencia = txg.REF_TRANS,
                                                IdLinea = txg.PROD_LINE == "" ? null : txg.PROD_LINE,
                                                IdTipoPedido = tipoPedido?.IdTipoPedido,
                                            };
                                            context.FacturasxCliente.Add(acuerdo);
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
          
        }

        public static async void ReiniciarTaskActualizarLineas(string CodigoAsesor, string ClienteId)
        {


            TaskActualizarLineas = new Task(async () =>
            {

                //List<Clientes> clientes = new List<Clientes>();
                var clientes = new List<AsesorXClienteViewModel>();

                

                List<FacturasXClienteApiModel> facturasXCliente = new List<FacturasXClienteApiModel>();
                HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, CodigoAsesor, ClienteId)).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    facturasXCliente = await response.Content.ReadAsAsync<List<FacturasXClienteApiModel>>();
                    Parallel.ForEach(facturasXCliente, txg =>
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            try
                            {
                                var tipoPedido =
                                    context.TiposdePedido.FirstOrDefault(tp =>
                                        tp.TipoPedido == txg.DOC_TYPE);

                                var factura = context.FacturasxCliente.FirstOrDefault(fac => fac.Referencia == txg.REF_TRANS);
                                bool shouldAdd = false;
                                if (factura == null)
                                {
                                    factura = new FacturasxCliente();
                                    shouldAdd = true;
                                }
                                factura.EmpresaId = txg.ENTITY;
                                factura.CodigoCliente = txg.ACCOUNT_NUM;
                                factura.Factura = txg.INVOICE;
                                factura.Tipo = txg.TRANS_TYPE;
                                factura.FechaFactura = txg.DOCUMENT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                factura.TotalFactura = txg.AMOUNT_CUR != null ? Convert.ToDecimal(txg.AMOUNT_CUR) : 0;
                                factura.Saldo = txg.REMAIN_AMOUNT_CUR != null ? Convert.ToDecimal(txg.REMAIN_AMOUNT_CUR) : 0;
                                factura.PendienteFactura = txg.AMOUNT_PENDING != null ? Convert.ToDecimal(txg.AMOUNT_PENDING) : 0;
                                factura.FechaVencimiento = txg.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                factura.FechaMaxDescuento = txg.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                factura.Descuento = txg.DISCOUNT != null ? Convert.ToDecimal(txg.DISCOUNT) : 0;
                                factura.IdMoneda = txg.CURRENCY_CODE;
                                factura.FacturaStatus = txg.STATUS;
                                factura.NumeroPagos = txg.N_PAYMENTS != null ? Convert.ToInt32(txg.N_PAYMENTS) : 0;
                                factura.Referencia = txg.REF_TRANS;
                                factura.IdLinea = txg.PROD_LINE == "" ? null : txg.PROD_LINE;
                                factura.IdTipoPedido = tipoPedido?.IdTipoPedido;
                                if (shouldAdd)
                                {
                                    context.FacturasxCliente.Add(factura);
                                }
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

                Debug.WriteLine("FFinalizo");

            });
        }

    }
}