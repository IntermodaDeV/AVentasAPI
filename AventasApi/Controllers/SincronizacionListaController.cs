using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/SincronizacionLista")]
    public class SincronizacionListaController : ApiController
    {
        [HttpGet]
        [Route("ModulosGestores")]
        public IHttpActionResult GetModulosGestores()
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
                    var modulos = context.MODULOS.ToList();
                    if (modulos.Count <= 0)
                    {
                        return NotFound();
                    }

                    var modulosList = modulos.Select(x => new ModuloModel()
                    {
                        ID = x.ID,
                        NOMBRE = x.NOMBRE
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
        [Route("Gestores")]
        public IHttpActionResult GetGestores()
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
                    var gestores = context.GESTORES.Where(x => x.ID_MODULO != null).ToList();
                    if (gestores.Count <= 0)
                    {
                        return NotFound();
                    }

                    var gestoresList = gestores.Select(x => new GestorModel()
                    {
                        DESCRIPCION = x.DESCRIPCION,
                        FORZAR = x.FORZAR,
                        GRUPO = x.GRUPO,
                        ID = x.ID,
                        ID_GP = x.ID_GP,
                        ID_MODULO = x.ID_MODULO,
                        NIVEL_PRIORIDAD = x.NIVEL_PRIORIDAD,
                        NOMBRE = x.ETIQUETA,
                        ORDEN = x.ORDEN,
                        ROWID = x.ROWID,
                        STATUS = x.STATUS,
                        T_MIN_SINC = x.T_MIN_SINC,
                        ULT_SINCRONIZACION = x.ULT_SINCRONIZACION
                    }
                    ).ToList();

                    return Ok(gestoresList);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("{idGestor}/{user}/upload")]
        public IHttpActionResult UploadSalesOrder(int? idGestor, string user)
        {
            try
            {
                using (var context = new AVentasConfigEntities())
                {
                    var respuesta = context.SP_POST_LISTA_EJECUCION_MANUAL(idGestor, user).ToList();

                    return Ok(respuesta.FirstOrDefault().Resultado);
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
                    var modulos = context.LISTA_EJECUCION_MANUAL.Where(x => x.USUARIO == usuario && x.FECHA >= fechaInicio).ToList();
                    if (modulos.Count <= 0)
                    {
                        return NotFound();
                    }

                    var modulosList = modulos.Select(x => new ListaEjecucionManuaVisuallModel()
                    {
                       ID = x.ID,
                       ID_GESTOR = x.ID_GESTOR,
                       NOMBRE = x.GESTORES.ETIQUETA,
                       FECHASTR = x.FECHA.HasValue ? x.FECHA.Value.ToString("dd/MM/yyyy hh:mm tt"): "",
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
