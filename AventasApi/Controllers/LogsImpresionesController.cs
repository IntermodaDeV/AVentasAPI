using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using AventasApi.Utils;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class LogsImpresionesController : ApiController
    {
        [HttpPost]
        [Route("~/api/logImpresionRecibo")]
        public async Task<IHttpActionResult> RegistrarLogRecibo([FromBody] LogRecibosViewModel logRecibos)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var recibo = db.RecibosxCliente.FirstOrDefault(r => r.NumeroRecibo == logRecibos.numRecibo);
                    var anticipo = db.AnticiposxCliente.FirstOrDefault(r => r.NumeroRecibo == logRecibos.numRecibo);

                    if (recibo == null && anticipo == null)
                    {
                        return NotFound();
                    }

                    int idRecibo = 0;
                    if (recibo != null)
                    {
                        recibo.Reimpresion = false;
                        idRecibo = recibo.ReciboId;
                    }

                    if (anticipo != null)
                    {
                        idRecibo = anticipo.AnticipoId;
                        anticipo.Reimpresion = false;
                    }
                    await db.SaveChangesAsync();

                    var LogRecibos = new LogRecibo() { 
                        ReciboId = idRecibo,
                        Usuario = logRecibos.Usuario,
                        Fecha = DateTime.Now,
                        Latitude = logRecibos.Latitude,
                        Longitude = logRecibos.longitude
                     };
                    db.LogRecibo.Add(LogRecibos);                 
                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                await ErrorLogger.LogErrorAsync(errorCode: "LIMPT01", controlador: "LogsImpresionesController", ruta: "api/logImpresionRecibo", usuario: "", mensaje: e.Message);
                return Content(HttpStatusCode.InternalServerError, new { ErrorCode = "LIMPT01", Message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        [HttpPost]
        [Route("~/api/logImpresionReciboProforma")]
        public async Task<IHttpActionResult> RegistrarLogProforma([FromBody] LogProformaViewModel logProforma)
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var ProformaId = db.RecibosProforma.FirstOrDefault(r => r.NumeroProforma == logProforma.numProforma).ProformaId;
                    var LogProformas = new LogProforma()
                    {
                        ProformaId = ProformaId,
                        Usuario = logProforma.Usuario,
                        Fecha = DateTime.Now,
                        Latitude = logProforma.Latitude,
                        Longitude = logProforma.longitude
                    };
                    db.LogProforma.Add(LogProformas);
                    var result = await db.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
