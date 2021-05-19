using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
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
                    var reciboId = db.RecibosxCliente.FirstOrDefault(r => r.NumeroRecibo == logRecibos.numRecibo).ReciboId;
                    var LogRecibos = new LogRecibo() { 
                        ReciboId = reciboId,
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
                return BadRequest(e.ToString());
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
