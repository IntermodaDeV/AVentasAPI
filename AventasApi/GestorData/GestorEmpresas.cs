using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using AventasApi.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.GestorData
{
    public class GestorEmpresas
    {
        private static readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}empresa/empresas";

        public static async Task ObtenerEmpresas()
        {
            LogicValidation LogicValidation = new LogicValidation();
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
                            foreach (var empresa in listaEmpresas)
                            {
                                if (LogicValidation.IsDataValid(empresa))
                                {
                                    using (AVentasEntities context = new AVentasEntities())
                                    {
                                        var empresaBD = context.Empresa.Find(empresa.COMPANY_CODE);
                                        if (LogicValidation.IsDataValid(empresaBD))
                                        {
                                            context.Entry(empresaBD).State = EntityState.Modified;
                                            empresaBD.EmpresaId = empresa.COMPANY_CODE;
                                            empresaBD.NombreEmpresa = empresa.NAME;
                                            empresaBD.Direccion = empresa.ADDRESS;
                                            empresaBD.RegistroTributario = empresa.NIFCIF;
                                        }
                                        else
                                        {
                                            Empresa nuevaEmpresa = new Empresa()
                                            {
                                                EmpresaId = empresa.COMPANY_CODE,
                                                NombreEmpresa = empresa.NAME,
                                                Direccion = empresa.ADDRESS,
                                                RegistroTributario = empresa.NIFCIF
                                            };
                                            context.Empresa.Add(nuevaEmpresa);
                                        }

                                        try
                                        {
                                            context.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                        }
                                    }

                                }

                            }
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
    }
}