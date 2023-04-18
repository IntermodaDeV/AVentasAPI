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
    public class SyncRutas
    {
        public async Task SincronizarRurasAsesor()
        {
            try
            {
                var asesores = await GetAsesoresActivos();

                foreach (var asesor in asesores)
                {
                    var rutas = new List<RutasCRMApiModel>();
                    var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                    var request = new RestRequest($"asesor/{asesor.EmpresaId}/{asesor.Diario}/Rutas", Method.GET);
                    request.AddHeader("Accept", "application/json");
                    IRestResponse response = restClient.Execute(request);

                    if (response.IsSuccessful && response.Content != "null")
                    {
                        rutas = JsonConvert.DeserializeObject<List<RutasCRMApiModel>>(response.Content);

                        if(rutas.Count > 0)
                        {
                          await  Rutas(rutas, asesor);
                        }
                    }
                }
            }
            catch (Exception )
            {
                throw;
            }
        }

        private async Task Rutas(List<RutasCRMApiModel> rutas, Asesores asesor)
        {
            try
            {
                foreach (var _ruta in rutas)
                {

                    Rutas ruta = new Rutas
                    {
                        CodigoRuta = asesor.EmpresaId.ToUpper() + "-" + _ruta.CODE,
                        EmpresaId = _ruta.ENTITY,
                        Nombre = _ruta.DESCRIPTION,
                        Revision = null
                    };

                   await SaveAndUpdateRuta(ruta);

                    RutasxAsesor rutasAsesor = new RutasxAsesor
                    {
                        CodigoRuta = asesor.EmpresaId.ToUpper() + "-" + _ruta.CODE,
                        CodigoAsesor = asesor.CodigoAsesor,
                    };

                   await SaveAndUpdateRutasxAsesor(rutasAsesor);

                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task SaveAndUpdateRuta(Rutas ruta)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var result = await db.Rutas.FirstOrDefaultAsync(p => p.CodigoRuta == ruta.CodigoRuta);

                    if(result == null)
                    {
                        db.Rutas.Add(ruta);
                    }
                    else
                    {
                        result.Nombre = ruta.Nombre;
                        result.Revision = ruta.Revision;
                    }
                   await  db.SaveChangesAsync();

                }
             }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task SaveAndUpdateRutasxAsesor(RutasxAsesor rutaAsesor)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var result = await db.RutasxAsesor.FirstOrDefaultAsync(p => p.CodigoRuta == rutaAsesor.CodigoRuta && p.CodigoAsesor == rutaAsesor.CodigoAsesor);

                    if (result == null)
                    {
                        db.RutasxAsesor.Add(rutaAsesor);
                    }
                    
                    await db.SaveChangesAsync();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        private async Task<List<Asesores>> GetAsesoresActivos()
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    
                   return await db.Asesores.Where(x => x.Activo == true).ToListAsync();
                   
                }               
            }
            catch (Exception)
            {
                throw;
            }           
        }
    }
}