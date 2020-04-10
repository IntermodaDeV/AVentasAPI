using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ExternalApiData.GestorData
{
    public class GestorSaldosCliente
    {
        public async Task<string> ActualizarSaldos(string clienteId)
        {
            Asesores asesor = new Asesores();
            using (AVentasEntities context = new AVentasEntities())
            {
                asesor = context.ClientesxRuta.AsNoTracking().FirstOrDefault(cliXRut => cliXRut.CodigoCliente == clienteId)?.Rutas.RutasxAsesor.FirstOrDefault()?.Asesores;
                if (asesor == null)
                {
                    throw new Exception("No se encontro asesor para ese cliente.");
                }
            }
            Task actualizarCliente = new Task(async () =>
            {
                Clientes cliente = new Clientes();
                GestorClientes gestorClientes = new GestorClientes();
                cliente = gestorClientes.ObtenerClientePorId(clienteId, asesor.CodigoAsesor, asesor.EmpresaId).Result;
                if (cliente == null)
                {
                    throw new Exception("Error en la peticion de cliente.");
                }
                using (AVentasEntities context = new AVentasEntities())
                {
                    var clienteBD = context.Clientes.FirstOrDefault(cli => cli.CodigoCliente == cliente.CodigoCliente);
                    if (clienteBD == null)
                    {
                        throw new Exception("Cliente No existe");
                    }
                    clienteBD.CodigoCliente = cliente.CodigoCliente;
                    clienteBD.EmpresaId = cliente.EmpresaId;
                    clienteBD.Nombre = cliente.Nombre;
                    clienteBD.ComunidadAutonoma = cliente.ComunidadAutonoma;
                    clienteBD.GrupoPrecio = cliente.GrupoPrecio;
                    clienteBD.GrupoCliente = cliente.GrupoCliente;
                    clienteBD.Descuento = cliente.Descuento;
                    clienteBD.Direccion = cliente.Direccion;
                    clienteBD.IdMoneda = cliente.IdMoneda;
                    clienteBD.FacturacionEntrega = cliente.FacturacionEntrega;
                    await context.SaveChangesAsync();
                }
            });
            Task actualizarAcuerdosXCliente = new Task(async () =>
            {
                GestorAcuerdosVenta gestorAcuerdosVenta = new GestorAcuerdosVenta();
                var acuerdosXCliente = gestorAcuerdosVenta.ObtenerAcuerdosxCliente(clienteId, asesor.CodigoAsesor, asesor.EmpresaId);
                gestorAcuerdosVenta.ModificarOAgregarAcuerdos(acuerdosXCliente, clienteId).Wait();
            });
            Task actualizarFacturas = new Task(async () =>
            {
                string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/imhn/1/{{0}}/{{1}}/FactCliente";
                HttpClient client = new ClienteHttp();
                List<FacturasXClienteApiModel> facturasXCliente = new List<FacturasXClienteApiModel>();
                HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, asesor.CodigoAsesor, clienteId)).ConfigureAwait(false);
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
                    throw new Exception("Error En la peticion de Facturas");
                }
            });
            Task actualizarSubFacuras = new Task(async () =>
            {
                string UrlString = $"{Enviroment.CRMWebServiceURLApi}facturas/IMHN/gmonrroy/gmonrroy/0/{{0}}";
                HttpClient client = new ClienteHttp();
                List<SubFacturasXClienteApiModel> facturasXCliente = new List<SubFacturasXClienteApiModel>();
                //HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, "IMHN-000000272")).ConfigureAwait(false);
                HttpResponseMessage response = await client.GetAsync(string.Format(UrlString, clienteId)).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    facturasXCliente = await response.Content.ReadAsAsync<List<SubFacturasXClienteApiModel>>();
                    Parallel.ForEach(facturasXCliente, txg =>
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            var factura =
                                context.FacturasxCliente.FirstOrDefault(fac =>
                                    fac.Referencia == txg.REF_CUSTTRANS);
                            try
                            {
                                var subFactura = context.SubFacturasxCliente.FirstOrDefault(subfac => subfac.Referencia == txg.REF_TRANSOPEN);
                                bool shouldAdd = true;
                                if (subFactura == null)
                                {
                                    subFactura = new SubFacturasxCliente();
                                    shouldAdd = false;
                                }

                                subFactura.EmpresaId = txg.ENTITY;
                                subFactura.CodigoCliente = txg.ACCOUNT_NUM;
                                subFactura.Factura = txg.INVOICE;
                                subFactura.Saldo = txg.AMOUNT_MST != null ? Convert.ToDecimal(txg.AMOUNT_MST) : 0;
                                subFactura.SaldoDivisa = txg.AMOUNT_CUR != null ? Convert.ToDecimal(txg.AMOUNT_CUR) : 0;
                                subFactura.FechaVencimiento = txg.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                subFactura.FechaMaxDescuento = txg.LIMIT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.LIMIT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                subFactura.FechaVencimientoDescuento = txg.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(txg.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                subFactura.Descuento = txg.DISC_AMOUNT != null ? Convert.ToDecimal(txg.DISC_AMOUNT) : 0;
                                subFactura.PendientePago = txg.PAYM_AMOUNT != null ? Convert.ToDecimal(txg.PAYM_AMOUNT) : 0;
                                subFactura.Referencia = txg.REF_TRANSOPEN;
                                subFactura.ReferenciaFacturas = txg.REF_CUSTTRANS;
                                subFactura.ReferenciaAcuerdo = txg.AGREEMENT_NUM;
                                subFactura.NumeroCuota = txg.PA_PAYM_NUM != null ? Convert.ToInt32(txg.PA_PAYM_NUM) : 0;
                                subFactura.ValorCuota = txg.PA_PAYM_AMOUNT != null ? Convert.ToDecimal(txg.PA_PAYM_AMOUNT) : 0;
                                subFactura.ValorVencidoCuota = txg.PA_DUE_AMOUNT != null ? Convert.ToDecimal(txg.PA_DUE_AMOUNT) : 0;
                                subFactura.ReferenciaCuotas = txg.PA_REF_APSA;
                                subFactura.IdMoneda = txg.CURRENCY_CODE;
                                subFactura.IdAcuerdoxCliente = txg.AGREEMENT_NAME == "" ? null : txg.AGREEMENT_NAME;
                                subFactura.IdFactura = factura.IdFactura;


                                factura.IdAcuerdoxCliente = subFactura.IdAcuerdoxCliente;
                                if (!shouldAdd)
                                {
                                    context.SubFacturasxCliente.Add(subFactura);
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
                    throw new Exception("Error En la peticion de subFacturas");
                }
            });
            actualizarAcuerdosXCliente.Start();
            actualizarAcuerdosXCliente.Wait();
            actualizarCliente.Start();
            actualizarCliente.Wait();
            actualizarFacturas.Start();
            actualizarFacturas.Wait();
            actualizarSubFacuras.Start();
            actualizarSubFacuras.Wait();
            return "Se actualizo la info";
        }
    }
}