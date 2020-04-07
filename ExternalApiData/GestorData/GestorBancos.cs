using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Enviroments;
using DBData.Database;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace ExternalApiData.GestorData
{
    public class GestorBancos
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}bancos/imhn";
        private static LogicValidation LogicValidation = new LogicValidation();

        public static async Task ObtenerBancos()
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
                        List<BancoApiModel> listaBancos = JsonConvert.DeserializeObject<List<BancoApiModel>>(response.Content);

                        if (LogicValidation.ValidateDataCount(listaBancos.Count))
                        {
                            int updateCount = 0, insertCount = 0, errorCount = 0;
                            foreach (var banco in listaBancos)
                            {
                                if (LogicValidation.IsDataValid(banco))
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var bancoBD = context.Bancos.FirstOrDefault(x=>x.NombreBanco == banco.CODE);
                                        if (LogicValidation.IsDataValid(bancoBD))
                                        {
                                            updateCount++;
                                            context.Entry(bancoBD).State = EntityState.Modified;
                                            bancoBD.NombreBanco = banco.CODE;
                                            bancoBD.Descripcion = banco.DESCRIPTION;
                                            bancoBD.EmpresaId = banco.COMPANY_CODE;

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
                                            errorCount += CreandoBanco(banco);
                                        }
                                    }
                                }
                            }
                            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                            LogicValidation.EmailNotification("GestorBancos", counter);
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

        public static int CreandoBanco(BancoApiModel banco)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                Bancos nuevoBanco = new Bancos()
                {
                    NombreBanco = banco.CODE,
                    Descripcion = banco.DESCRIPTION,
                    EmpresaId = banco.COMPANY_CODE,
                };
                context.Bancos.Add(nuevoBanco);

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