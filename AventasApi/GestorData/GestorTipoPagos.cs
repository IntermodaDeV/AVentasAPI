using AventasApi.Enviroments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AventasApi.Utils;
using System.Web;
using RestSharp;
using AventasApi.Models.ApiModels;
using Newtonsoft.Json;
using DBData.Database;
using System.Data.Entity;

namespace AventasApi.GestorData
{
    public class GestorTipoPagos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}tipopagos/imhn";
        private static LogicValidation LogicValidation = new LogicValidation();

        public static async Task ObtenerTipos()
        {
            try
            {
                await Task.Run(() =>
                {
                    var restClient = new RestClient(UrlString);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Accept", "application/json");
                    IRestResponse response = restClient.Execute(request);

                    if (response.IsSuccessful)
                    {
                        List<TiposPagoCRMApiModel> listaTipos = JsonConvert.DeserializeObject<List<TiposPagoCRMApiModel>>(response.Content);

                        if (LogicValidation.ValidateDataCount(listaTipos.Count))
                        {
                            int updateCount = 0, insertCount = 0, errorCount = 0;
                            foreach (var tipo in listaTipos)
                            {
                                if (LogicValidation.IsDataValid(tipo))
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var tipoPago = context.TiposdePago.FirstOrDefault(x => x.Codigo == tipo.CODE);
                                        if (LogicValidation.IsDataValid(tipoPago))
                                        {
                                            updateCount++;
                                            context.Entry(tipoPago).State = EntityState.Modified;
                                            tipoPago.Codigo = tipo.CODE;
                                            tipoPago.Descripcion = tipo.DESCRIPTION;
                                            tipoPago.Tipo = tipo.TYPE;
                                            tipoPago.EmpresaId = tipo.COMPANY_CODE;

                                            try
                                            {
                                                context.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                errorCount++;
                                                Console.WriteLine(ex);
                                            }
                                        }
                                        else
                                        {
                                            insertCount++;
                                            errorCount += CreandoTipoPago(tipo);
                                        }
                                    }
                                }
                            }
                            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                            LogicValidation.EmailNotification("GestorTipoPagos", counter);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static int CreandoTipoPago(TiposPagoCRMApiModel tipo)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                TiposdePago nuevoTipo = new TiposdePago()
                {
                    Codigo = tipo.CODE,
                    Descripcion = tipo.DESCRIPTION,
                    Tipo = tipo.TYPE,
                    EmpresaId = tipo.COMPANY_CODE,
                };
                context.TiposdePago.Add(nuevoTipo);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    contador++;
                    Console.WriteLine(ex);
                }
            }
            return contador;
        }
    }
}