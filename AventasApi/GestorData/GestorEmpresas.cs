using AventasApi.Enviroments;
using DBData.Database;
using AventasApi.Models.ApiModels;
using AventasApi.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.GestorData
{
    public class GestorEmpresas
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}empresa/empresas";
        private static LogicValidation LogicValidation = new LogicValidation();

        public static async Task ObtenerEmpresas()
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
                        List<EmpresasCRMApiModel> listaEmpresas = JsonConvert.DeserializeObject<List<EmpresasCRMApiModel>>(response.Content);

                        if (LogicValidation.ValidateDataCount(listaEmpresas.Count))
                        {
                            int updateCount = 0, insertCount = 0, errorCount = 0;
                            foreach (var empresa in listaEmpresas)
                            {
                                if (LogicValidation.IsDataValid(empresa))
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var empresaBD = context.Empresa.Find(empresa.COMPANY_CODE);
                                        if (LogicValidation.IsDataValid(empresaBD))
                                        {
                                            updateCount++;
                                            context.Entry(empresaBD).State = EntityState.Modified;
                                            empresaBD.EmpresaId = empresa.COMPANY_CODE;
                                            empresaBD.NombreEmpresa = empresa.NAME;
                                            empresaBD.Direccion = empresa.ADDRESS;
                                            empresaBD.RegistroTributario = empresa.NIFCIF;

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
                                            errorCount += CreandoEmpresa(empresa);
                                        }
                                    }
                                }
                            }
                            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                            LogicValidation.EmailNotification("GestorEmpresas", counter);
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

        public static int CreandoEmpresa(EmpresasCRMApiModel empresa)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                Empresa nuevaEmpresa = new Empresa()
                {
                    EmpresaId = empresa.COMPANY_CODE,
                    NombreEmpresa = empresa.NAME,
                    Direccion = empresa.ADDRESS,
                    RegistroTributario = empresa.NIFCIF,
                    Revision = null
                };
                context.Empresa.Add(nuevaEmpresa);

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