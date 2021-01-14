using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExternalApiData.Enviroments;
using RestSharp;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/SincronizacionEspecifico")]
    public class SincronizacionEspecificoController : ApiController
    {
        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("Coleccion/upload")]
        public IHttpActionResult EnviarLista([FromBody] PosteoColeccionSincEspecificoModel model)
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
                    var found = context.LISTA_EJECUCION_ESPECIFICO.Where(x => x.ID_GESTOR == model.IdGestor && x.FINALIZADO == false).ToList();

                    if(found != null)
                    {

                        foreach(var iterlista in found)
                        {
                            string empresa = "";
                            string coleccion = "";

                            foreach(var iterpar in iterlista.PARAMETROS_LE_ESPECIFICO)
                            {
                                if(iterpar.TIPO == "COLECCION")
                                {
                                    coleccion = iterpar.VALOR;
                                }

                                if(iterpar.TIPO == "EMPRESA")
                                {
                                    empresa = iterpar.VALOR;
                                }
                            }
                            
                            if(empresa == model.EmpresaId && coleccion == model.ColeccionId)
                            {
                                return BadRequest($"Colección ya se encuentra en lista de espera o ejecución.");
                            }


                        }
                    }

                    var entityLista = new LISTA_EJECUCION_ESPECIFICO
                    {
                        ID_GESTOR = model.IdGestor,
                        EN_ESPERA = true,
                        EN_EJECUCION = false,
                        FINALIZADO = false,
                        FECHA = DateTime.Now,
                        USUARIO = model.Usuario
                    };

                    context.LISTA_EJECUCION_ESPECIFICO.Add(entityLista);
                    context.SaveChanges();

                    if (entityLista.ID != 0)
                    {
                        var entityParametros1 = new PARAMETROS_LE_ESPECIFICO
                        {
                            ID_LISTA_EJECUCION = entityLista.ID,
                            FECHA = DateTime.Now,
                            TIPO = "EMPRESA",
                            VALOR = model.EmpresaId,
                            USUARIO = model.Usuario
                        };

                        var entityParametros2 = new PARAMETROS_LE_ESPECIFICO
                        {
                            ID_LISTA_EJECUCION = entityLista.ID,
                            FECHA = DateTime.Now,
                            TIPO = "COLECCION",
                            VALOR = model.ColeccionId,
                            USUARIO = model.Usuario
                        };

                        var entityParametros3 = new PARAMETROS_LE_ESPECIFICO
                        {
                            ID_LISTA_EJECUCION = entityLista.ID,
                            FECHA = DateTime.Now,
                            TIPO = "FORZAR",
                            VALOR = model.Forzar,
                            USUARIO = model.Usuario
                        };

                        context.PARAMETROS_LE_ESPECIFICO.Add(entityParametros1);
                        context.SaveChanges();
                        context.PARAMETROS_LE_ESPECIFICO.Add(entityParametros2);
                        context.SaveChanges();
                        context.PARAMETROS_LE_ESPECIFICO.Add(entityParametros3);
                        context.SaveChanges();
                    }

                    return Ok();
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("Coleccion/cancelar")]
        public IHttpActionResult CancelarLista([FromBody] PosteoCancelarListaEspecificoModel model)
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
                    var lista = context.LISTA_EJECUCION_ESPECIFICO.Where(x => x.ID == model.IdLista);

                    if(lista != null)
                    {
                        var listaACancelar = lista.FirstOrDefault();

                        if (listaACancelar != null)
                        {
                            //vuelve a actualizar el registro para verificar que no esté en ejecución
                            listaACancelar = context.LISTA_EJECUCION_ESPECIFICO.Where(x => x.ID == model.IdLista).FirstOrDefault();

                            if (listaACancelar.EN_ESPERA == true && listaACancelar.EN_EJECUCION != true)
                            {
                                listaACancelar.EN_ESPERA = false;
                                listaACancelar.EN_EJECUCION = false;
                                listaACancelar.FINALIZADO = true;

                                context.Entry(listaACancelar).State = EntityState.Modified;
                                context.SaveChanges();

                                return Ok();
                            }
                            else
                            {
                                return BadRequest($"Lista ID: {model.IdLista} ya se encuentra en ejecución, no es posible cancelarla.");
                            }
                        }
                        else
                        {
                            return BadRequest($"No se encontró el registro de Lista ID: {model.IdLista}.");
                        }
                    }    
                    else
                    {
                        return BadRequest($"No se encontró el registro de Lista ID: {model.IdLista}.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{usuario}")]
        public IHttpActionResult GetListado(string usuario)
        {
            try
            {
                //var fecha = DateTime.ParseExact(date, "yyyyMMddHHmmss", null);
                var fechaInicio = DateTime.Now.Date.AddDays(-15);

                using (var context = new AVentasConfigEntities())
                {                   
                    var modulos = context.LISTA_EJECUCION_ESPECIFICO.Where(x => x.USUARIO == usuario && x.FECHA >= fechaInicio).ToList();
                    if (modulos.Count <= 0)
                    {
                        return NotFound();
                    }

                    var modulosList = modulos.Select(x => new ListaEjecucionManuaVisuallModel()
                    {
                        ID = x.ID,
                        ID_GESTOR = x.ID_GESTOR,
                        NOMBRE = x.GESTORES.ETIQUETA,
                        FECHASTR = x.FECHA.HasValue ? x.FECHA.Value.ToString("dd/MM/yyyy hh:mm tt") : "",
                        FECHA = x.FECHA,
                        USUARIO = x.USUARIO,
                        EN_ESPERA = x.EN_ESPERA,
                        EN_EJECUCION = x.EN_EJECUCION,
                        FINALIZADO = x.FINALIZADO,
                        ID_MODULO = x.GESTORES.MODULOS.ID,
                        MODULO = x.GESTORES.MODULOS.NOMBRE,
                        PAQUETE = context.PARAMETROS_LE_ESPECIFICO.FirstOrDefault(p => p.ID_LISTA_EJECUCION == x.ID && p.TIPO == "COLECCION").VALOR,
                        EMPRESA = context.PARAMETROS_LE_ESPECIFICO.FirstOrDefault(p => p.ID_LISTA_EJECUCION == x.ID && p.TIPO == "EMPRESA").VALOR
                    }
                    ).ToList();

                    return Ok(modulosList);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("verificar/{empresa}/{paquete}")]
        public IHttpActionResult VerificarPaquete(string paquete, string empresa)
        {
            try
            {
                var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}paquetes/{empresa}/{paquete}/existe");
                client.Timeout = 480 * (1000);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    return BadRequest("Servidor se encuentra fuera de linea.");
                }

                var content = Newtonsoft.Json.JsonConvert.DeserializeObject<ColeccionApiModel>(response.Content);
                return Ok(content);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }

    public class ColeccionApiModel
    {
        public string CodigoPaquete { get; set; }
    }
}