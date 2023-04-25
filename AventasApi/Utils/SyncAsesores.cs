using AventasApi.Models.Authentication;
using DBData.Database;
using ExternalApiData.ApiModels;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.Utils
{
    public class SyncAsesores
    {
        public async Task SincronizacionAsesores()
        {
            try
            {   
                var asesores = new List<AsesoresCRMApiModel>();
                var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"asesor/AsesoresDisponibles", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    asesores = JsonConvert.DeserializeObject<List<AsesoresCRMApiModel>>(response.Content);

                    if (asesores.Count > 0)
                    {
                      await SaveAndUpdateAsesores(asesores);
                    }   
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private async Task SaveAndUpdateAsesores(List<AsesoresCRMApiModel> asesores)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    foreach (var asesor in asesores)
                    {
                        var entity = await db.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor == asesor.CODE && x.EmpresaId == asesor.ENTITY);

                        var fullName = asesor.NAME.Split(' ');
                        var iniciales = string.Empty;
                        foreach (var caracter in fullName)
                        {
                            if (caracter != string.Empty)
                            {
                                iniciales += caracter.Substring(0, 1).ToUpper();
                            }
                        }

                        if (entity == null)
                        {
                            Asesores newAsesor = new Asesores
                            {
                                CodigoAsesor = asesor.CODE,
                                EmpresaId = asesor.ENTITY,
                                Usuario = asesor.CODE,
                                Nombre = asesor.NAME,
                                Diario = asesor.JOURNAL,
                                InicialesNombre = iniciales

                            };
                            db.Asesores.Add(newAsesor);
                        }
                        else
                        {
                            entity.CodigoAsesor = asesor.CODE;
                            entity.EmpresaId = asesor.ENTITY;
                            entity.Usuario = asesor.CODE;
                            entity.Nombre = asesor.NAME;
                            entity.Diario = asesor.JOURNAL;
                            entity.InicialesNombre = iniciales;
                        }

                       await db.SaveChangesAsync();

                    }
                } 
            }
            catch (Exception e)
            {
               
            }
        }
    }
}