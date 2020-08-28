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
                        NOMBRE = x.NOMBRE,
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
    }
}
