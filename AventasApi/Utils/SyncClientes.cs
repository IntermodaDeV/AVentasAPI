using DBData.Database;
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
    public class SyncClientes
    {
        public void SyncCliente(string empresa, string codigoCliente, string codigoAsesor)
        {
            try
            {
                var clientes = new List<ClientesCRMApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"clientes/{empresa}/{codigoAsesor}/{codigoCliente}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);
                }

                if (clientes.Count > 0)
                {
                    UpdateCliente(clientes);
                }

            }
            catch (Exception e)
            {

            }
        }

        private void UpdateCliente(List<ClientesCRMApiModel> cliente)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    foreach (var cli in cliente)
                    {
                        var entityFound = db.Clientes.FirstOrDefault(x=> x.CodigoCliente == cli.ACCOUNT && x.EmpresaId == cli.ENTITY);
                        decimal cLimite = 0, cDisponible = 0;

                        if (entityFound != null)
                        {
                            entityFound.CodigoCliente = cli.ACCOUNT;
                            entityFound.EmpresaId = cli.ENTITY;
                            entityFound.Nombre = cli.NAME;
                            entityFound.Zona = cli.SALES_AREA;
                            entityFound.ComunidadAutonoma = cli.AUTONOMOUS_COMMUNITY;
                            entityFound.GrupoPrecio = cli.PRICE;
                            entityFound.GrupoCliente = cli.CUSTOMER_GROUP;
                            //entityFound.Descuento = cli.DISCOUNT_GROUP;
                            entityFound.Direccion = cli.ADDRESS;
                            entityFound.IdMoneda = cli.CURRENCY;
                            entityFound.FacturacionEntrega = cli.BLOCKED;
                            //entityFound.IncluyeImpuesto = cli.INCLUDE_TAX == "Sí";
                            entityFound.Provincias = null;
                            entityFound.Region = null;
                            entityFound.Revision = null;
                            entityFound.LimiteCredito = Decimal.TryParse(cli.CREDIT_LIMIT, out cLimite) ? cLimite : 0;
                            entityFound.CreditoDisponible = Decimal.TryParse(cli.CREDIT_LIMIT, out cDisponible) ? cDisponible : 0;
                            //entityFound.ModoEntrega = cli.DLVMODE;
                            entityFound.Telefono = cli.PHONE;
                            entityFound.GrupoImpuesto = cli.TAX_GROUP;
                            entityFound.Habilitado = true;
                            //entityFound.IgnorarSeqFact = cli.FLAG_SEQFACT == "Sí" ? true : false;


                            db.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                       
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}