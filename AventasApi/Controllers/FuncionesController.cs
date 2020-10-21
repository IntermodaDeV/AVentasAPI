using DBData.Database;
using System;
using AventasApi.Models.ViewModels;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class FuncionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        //[Route("api/OFunciones")]
        [HttpGet]
        public async Task<IHttpActionResult> GetFunciones()
        {
            try
            {
                var Funciones = context.Funciones.Where(f => f.Status == true).Select(f => new FuncionesViewModel
                {
                    IdFuncion = f.Id
                     , Nombre = f.Nombre
                     , Status = f.Status
                });
                return Ok(Funciones);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        public async Task<IHttpActionResult> Post(FuncionesViewModel Funcion)
        {
            try
            {
                var FuncionesDB = new Funciones 
                {
                    Nombre = Funcion.Nombre
                   ,Status = Funcion.Status
                };
                context.Funciones.Add(FuncionesDB);
                context.SaveChanges();
                var response = new { Message = "Se ha registrado con exito." };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
