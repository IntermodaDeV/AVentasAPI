using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Models.ApiModels;
using System.Diagnostics;
using RestSharp;
using Newtonsoft.Json;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorAcuerdosVenta
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}acuerdos/{{0}}/{{1}}/{{2}}";//0= entity,1=usuario,2=date 20190101 


        //private static AVentasEntities context = new AVentasEntities();



        public List<AcuerdosxCliente> ObtenerAcuerdosxCliente()
        {
            List<Asesores> asesores = new List<Asesores>();
            List<TiposdePedido> tiposPedido = new List<TiposdePedido>();
            List<AcuerdosxCliente> acuerdosAGuardar = new List<AcuerdosxCliente>();
            using (AVentasEntities context = new AVentasEntities())
            {
                asesores = context.Asesores.AsNoTracking().Where(ase => ase.EmpresaId == "imhn").ToList();
                tiposPedido = context.TiposdePedido.AsNoTracking().ToList();
            }

            Parallel.ForEach(asesores, ase =>
            {
                string peticion = string.Format(UrlString, ase.EmpresaId, ase.Usuario, "20190101") + "/Incremental";
                var restClient = new RestClient(peticion);
                restClient.Timeout = 600 * 1000;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    List<AcuerdosxCliente> acuerdoXAsesorsAGuardar = new List<AcuerdosxCliente>();

                    var acuerdos = JsonConvert.DeserializeObject<List<AcuerdoCRMApiModel>>(response.Content);
                    if (acuerdos == null)
                        acuerdos = new List<AcuerdoCRMApiModel>();
                    foreach (var acu in acuerdos)
                    {
                        var tipoPedido = tiposPedido.FirstOrDefault(tp => tp.TipoPedido == acu.CLASS_SALES_AGREEMENT);
                        decimal total, saldo, liberado, entregado, facturado = 0;
                        decimal.TryParse(acu.AMOUNT, out total);
                        decimal.TryParse(acu.REMAINING, out saldo);
                        decimal.TryParse(acu.RELEASED, out liberado);
                        decimal.TryParse(acu.DELIVERED, out entregado);
                        decimal.TryParse(acu.INVOICED, out facturado);
                        var acuerdoAGuardar = new AcuerdosxCliente
                        {
                            IdAcuerdoxCliente = acu.ID_SALES_AGREEMENT,
                            CodigoCliente = acu.CUSTOMER_ACCOUNT,
                            IdTipoPedido = tipoPedido.IdTipoPedido,
                            IdMoneda = acu.CURRENCY,
                            EmpresaId = acu.ENTITY,
                            Total = total,
                            Saldo = saldo,
                            Liberado = liberado,
                            Facturado = facturado,
                            Entregado = entregado,
                            //IdLinea = acu.,
                        };
                        if (!acuerdoXAsesorsAGuardar.Any(acuer => acuer.IdAcuerdoxCliente == acuerdoAGuardar.IdAcuerdoxCliente))
                            acuerdoXAsesorsAGuardar.Add(acuerdoAGuardar);
                    }
                    lock (acuerdosAGuardar)
                    {
                        acuerdoXAsesorsAGuardar.ForEach(acuer =>
                        {
                            if (!acuerdosAGuardar.Any(acuerd => acuer.IdAcuerdoxCliente == acuerd.IdAcuerdoxCliente))
                                acuerdosAGuardar.Add(acuer);
                        });
                    }
                }
            });
            return acuerdosAGuardar;
        }
        public List<AcuerdosxCliente> ObtenerAcuerdosxCliente(string clienteID, string usuario, string empresaIdr)
        {
            List<TiposdePedido> tiposPedido = new List<TiposdePedido>();
            List<AcuerdosxCliente> acuerdosAGuardar = new List<AcuerdosxCliente>();
            using (AVentasEntities context = new AVentasEntities())
            {
                tiposPedido = context.TiposdePedido.AsNoTracking().ToList();
            }
            string peticion = string.Format(UrlString, empresaIdr, usuario, clienteID);
            var restClient = new RestClient(peticion);
            restClient.Timeout = 600 * 1000;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                List<AcuerdosxCliente> acuerdoXAsesorsAGuardar = new List<AcuerdosxCliente>();

                var acuerdos = JsonConvert.DeserializeObject<List<AcuerdoCRMApiModel>>(response.Content);
                if (acuerdos == null)
                    acuerdos = new List<AcuerdoCRMApiModel>();
                foreach (var acu in acuerdos)
                {
                    var tipoPedido = tiposPedido.FirstOrDefault(tp => tp.TipoPedido == acu.CLASS_SALES_AGREEMENT);
                    decimal total, saldo, liberado, entregado, facturado = 0;
                    decimal.TryParse(acu.AMOUNT, out total);
                    decimal.TryParse(acu.REMAINING, out saldo);
                    decimal.TryParse(acu.RELEASED, out liberado);
                    decimal.TryParse(acu.DELIVERED, out entregado);
                    decimal.TryParse(acu.INVOICED, out facturado);
                    var acuerdoAGuardar = new AcuerdosxCliente
                    {
                        IdAcuerdoxCliente = acu.ID_SALES_AGREEMENT,
                        CodigoCliente = acu.CUSTOMER_ACCOUNT,
                        IdTipoPedido = tipoPedido.IdTipoPedido,
                        IdMoneda = acu.CURRENCY,
                        EmpresaId = acu.ENTITY,
                        Total = total,
                        Saldo = saldo,
                        Liberado = liberado,
                        Facturado = facturado,
                        Entregado = entregado,
                        //IdLinea = acu.,
                    };
                    if (!acuerdoXAsesorsAGuardar.Any(acuer => acuer.IdAcuerdoxCliente == acuerdoAGuardar.IdAcuerdoxCliente))
                        acuerdoXAsesorsAGuardar.Add(acuerdoAGuardar);
                }
                lock (acuerdosAGuardar)
                {
                    acuerdoXAsesorsAGuardar.ForEach(acuer =>
                    {
                        if (!acuerdosAGuardar.Any(acuerd => acuer.IdAcuerdoxCliente == acuerd.IdAcuerdoxCliente))
                            acuerdosAGuardar.Add(acuer);
                    });
                }
            }

            return acuerdosAGuardar;
        }
        public async Task GuardarAcuerdos(List<AcuerdosxCliente> acuerdosAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.AcuerdosxCliente.AddRange(acuerdosAGuardar);
                await context.SaveChangesAsync();
            }
        }
        public async Task ModificarOAgregarAcuerdos(List<AcuerdosxCliente> acuerdosAGuardar, string clienteID)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                List<string> idsAcuerdosXCliente = acuerdosAGuardar.Select(acuAGua => acuAGua.IdAcuerdoxCliente).ToList();
                var acuerdos = context.AcuerdosxCliente.Where(acuXCli => idsAcuerdosXCliente.Contains(acuXCli.IdAcuerdoxCliente));
                foreach (var acuerdoAGuardar in acuerdosAGuardar)
                {
                    var acuerdo = acuerdos.FirstOrDefault(acu => acu.IdAcuerdoxCliente == acuerdoAGuardar.IdAcuerdoxCliente);
                    if (acuerdo == null)
                    {
                        context.AcuerdosxCliente.Add(acuerdoAGuardar);
                    }
                    else
                    {
                        acuerdo.IdAcuerdoxCliente = acuerdoAGuardar.IdAcuerdoxCliente;
                        acuerdo.CodigoCliente = acuerdoAGuardar.CodigoCliente;
                        acuerdo.IdTipoPedido = acuerdoAGuardar.IdTipoPedido;
                        acuerdo.IdMoneda = acuerdoAGuardar.IdMoneda;
                        acuerdo.EmpresaId = acuerdoAGuardar.EmpresaId;
                        acuerdo.Total = acuerdoAGuardar.Total;
                        acuerdo.Saldo = acuerdoAGuardar.Saldo;
                        acuerdo.Liberado = acuerdoAGuardar.Liberado;
                        acuerdo.Facturado = acuerdoAGuardar.Facturado;
                        acuerdo.Entregado = acuerdoAGuardar.Entregado;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}