using AventasApi.Enviroments;
using System;
using System.Collections.Generic;
using System.Linq;
using AventasApi.Utils;
using System.Threading.Tasks;
using System.Web;
using RestSharp;
using Newtonsoft.Json;
using AventasApi.Models.ApiModels;
using AventasApi.Infrastructure;
using System.Data.Entity;

namespace AventasApi.GestorData
{
    public class GestorCuentasBancarias
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}cuentasbancarias/imhn";
        private static LogicValidation LogicValidation = new LogicValidation();

        public static async Task ObtenerCuentas()
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
                        List<CuentasBancariasCRMApiModel> cuentasBancarias = JsonConvert.DeserializeObject<List<CuentasBancariasCRMApiModel>>(response.Content);

                        if (LogicValidation.ValidateDataCount(cuentasBancarias.Count))
                        {
                            int updateCount = 0, insertCount = 0, errorCount = 0;
                            foreach (var cuenta in cuentasBancarias)
                            {
                                if (LogicValidation.IsDataValid(cuenta))
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var cuentaBD = context.CuentasBancarias.FirstOrDefault(x => x.NumeroCuenta == (cuenta.ACCOUNT_NUM ?? " "));
                                        var banco =
                                        context.Bancos.FirstOrDefault(x => x.NombreBanco == (cuenta.BANK_GROUP ?? " ") || 
                                        x.Descripcion == (cuenta.BANK_GROUP ?? " ") || x.Descripcion == (cuenta.DESCRIPTION ?? " ")) ??
                                        context.Bancos.FirstOrDefault(x => x.NombreBanco.Contains(cuenta.BANK_GROUP)) ??
                                        context.Bancos.FirstOrDefault(x => x.Descripcion == cuenta.DESCRIPTION);

                                        if (LogicValidation.IsDataValid(cuentaBD))
                                        {
                                            updateCount++;
                                            context.Entry(cuentaBD).State = EntityState.Modified;
                                            cuentaBD.NombreBanco = cuenta.CODE;
                                            cuentaBD.NumeroCuenta = cuenta.ACCOUNT_NUM;
                                            cuentaBD.Descripcion = cuenta.DESCRIPTION;
                                            cuentaBD.GrupoBanco = cuenta.BANK_GROUP;
                                            cuentaBD.IdBanco = banco?.IdBanco; 
                                            cuentaBD.IdMoneda = cuenta.CURRENCY;
                                            cuentaBD.EmpresaId = cuenta.COMPANY_CODE;

                                            try
                                            {
                                                context.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                updateCount--;
                                                errorCount++;
                                                Console.WriteLine(ex);
                                            }
                                        }
                                        else
                                        {
                                            insertCount++;
                                            CuentasBancarias nuevaCuenta = new CuentasBancarias()
                                            {
                                                NombreBanco = cuenta.CODE,
                                                NumeroCuenta = cuenta.ACCOUNT_NUM,
                                                Descripcion = cuenta.DESCRIPTION,
                                                GrupoBanco = cuenta.BANK_GROUP,
                                                IdBanco = banco?.IdBanco,
                                                IdMoneda = cuenta.CURRENCY,
                                                EmpresaId = cuenta.COMPANY_CODE,
                                            };
                                            context.CuentasBancarias.Add(nuevaCuenta);

                                            try
                                            {
                                                context.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                insertCount--;
                                                errorCount++;
                                                Console.WriteLine(ex);
                                            }
                                        }
                                    }
                                }
                            }
                            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                            LogicValidation.EmailNotification("GestorCuentasBancarias", counter);
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

        public static string GrupoBanco(string grupoBanco)
        {
            LogicValidation LogicValidation = new LogicValidation();
            var valor = " ";
            if (LogicValidation.IsDataValid(grupoBanco))
            {
                string[] banco = grupoBanco.Split('-');
                if (LogicValidation.ValidateDataCountWithRestriction(banco.Count(), 1))
                {
                    valor = banco[0] + " " + banco[1];
                }
            }
            return valor;
        }
    }    
}