using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/SincronizacionEspecifico")]
    public class SincronizacionEspecificoController : ApiController
    {
        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("Coleccion/upload")]
        public IHttpActionResult UploadSalesOrder([FromBody] PosteoColeccionSincEspecificoModel model)
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
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

                        context.PARAMETROS_LE_ESPECIFICO.Add(entityParametros1);
                        context.SaveChanges();
                        context.PARAMETROS_LE_ESPECIFICO.Add(entityParametros2);
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
                        MODULO = x.GESTORES.MODULOS.NOMBRE
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
    }
}