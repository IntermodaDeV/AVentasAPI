using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/devolucion")]
    public class DevolucionController : ApiController
    {
        [HttpPost]
        public IHttpActionResult PostDevolucion([FromBody]DevolucionPostModel devolucion)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    return Ok(devolucion);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
