using AventasApi.GestorData;
using AventasApi.Infrastructure;
using AventasApi.Singleton;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class GestorController : ApiController
    {

        [HttpGet]
        [Route("api/Gestor/ActualizarSaldos/{ClienteId}")]
        public async Task<IHttpActionResult> ActualizarSaldos(string ClienteId)
        {
            GestorSaldosCliente gestorSaldos = new GestorSaldosCliente();
            string status = await gestorSaldos.ActualizarSaldos(ClienteId);
            return Ok(status);


        }

        //[HttpGet]
        //[Route("api/Gestor/ActualizarColecciones")]
        //public async Task<IHttpActionResult> GetColecciones()
        //{
        //    if (GestorColecciones.TaskActualizarColecciones.Status != TaskStatus.Running)
        //    {
        //        try
        //        {
        //            if (GestorColecciones.TaskActualizarColecciones.Status != TaskStatus.Created)
        //            {
        //                GestorColecciones.ReiniciarTaskActualizarColecciones();
        //            }

        //            GestorColecciones.TaskActualizarColecciones.Start();
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine(e);
        //            throw;
        //        }
        //        return Ok("Se inicializo la Carga de Colecciones");
        //    }
        //    return BadRequest("Ya se estan actualizando las colecciones");

        //}

        //[HttpGet]
        //[Route("api/Gestor/base64")]
        //public async Task<IHttpActionResult> Getb64()
        //{
        //    return Ok(GestorDatas.firmab64);

        //}

        //[HttpGet]
        //[Route("api/Gestor/ActualizarAsesores")]
        //public async Task<IHttpActionResult> GetAsesores()
        //{
        //        GestorAsesores.ActualizarAsesores();

        //    return BadRequest("Ya se estan actualizando las colecciones");

        //}
        //[HttpGet]
        //[Route("api/Gestor/ActualizarProductos")]
        //public async Task<IHttpActionResult> GetProductos()
        //{
        //    GestorProductos.ActualizarProductos();
        //    return Ok("Se inicializo la Carga de Productos");
        //}
        //[HttpGet]
        //[Route("api/Gestor/ActualizarImagenes")]
        //public async Task<IHttpActionResult> Get()
        //{
        //    GestorImagenesXProducto.ActualizarImagenes();
        //    return Ok("Se inicializo la Carga de Imagenes");
        //}
        //[HttpGet]
        //[Route("api/Gestor/ActualizarClientes")]
        //public async Task<IHttpActionResult> GetClientes()
        //{
        //    GestorClientes.ActualizarClientes();
        //    return Ok("Se inicializo la Carga de Clientes");
        //}
        //[HttpGet]
        //[Route("api/Gestor/BorrarImagenes")]
        //public async Task<IHttpActionResult> BorrarImagenes()
        //{
        //    GestorImagenesXProducto.BorrarImagenes();
        //    return Ok("Se borraron las imagenes con exito.");
        //}
        //[HttpGet]
        //[Route("api/Gestor/Reseteo")]
        //public async Task<IHttpActionResult> Reseteo()
        //{

        //    using (AVentasEntities context = new AVentasEntities())
        //    {

        //        context.sp_ResetearTablas();
        //    }

        //    return Ok("Se Resetaron las Tablas con exito.");
        //}
    }
}
