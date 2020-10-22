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
                var Funciones = context.Funciones.Select(f => new FuncionesViewModel
                {
                    Id = f.Id
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
        [Route("~/api/Funciones/Crear")]
        public async Task<IHttpActionResult> Post([FromBody] FuncionesViewModel Funcion)
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

        [HttpPost]
        [Route("api/Funciones/ActualizarEstado/{Id}")]
        public async Task<IHttpActionResult> ActualizarEstado(int Id)
        {
            try
            {

                var FuncionesDB = await context.Funciones.FindAsync(Id);
                if(FuncionesDB == null)
                {
                    return BadRequest("No se encuentra la función.");
                }

                FuncionesDB.Status = !FuncionesDB.Status;
                context.Entry(FuncionesDB).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                var response = new { Message = "Se ha registrado con exito." };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/funcion/modificar")]
        public async Task<IHttpActionResult> ModificarFuncion([FromBody] FuncionesViewModel Funcion)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var FuncionDB = await ctx.Funciones.FindAsync(Funcion.Id);

                    if (FuncionDB == null)
                    {
                        return BadRequest("No se encuentra la Función");
                    }

                    FuncionDB.Nombre = Funcion.Nombre;
                    FuncionDB.Status = Funcion.Status;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
