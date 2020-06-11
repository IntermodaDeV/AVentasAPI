using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/clientecontado")]
    public class ClienteContadoController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpPost]
        public IHttpActionResult CreateClienteContado([FromBody] ClienteContado cliente)
        {
            try
            {
                if (cliente == null)
                {
                    return BadRequest();
                }
                context.ClienteContado.Add(cliente);
                context.SaveChanges();
                return Ok(cliente);
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }
        [HttpPost]
        [Route("edit")]
        public IHttpActionResult EditClienteContado([FromBody] ClienteContado cliente)
        {
            try
            {
                var item = context.ClienteContado.Find(cliente.id);
                if (item == null)
                {
                    return NotFound();
                }
                item.Nombre = cliente.Nombre;
                item.RTN = cliente.RTN;
                item.Telefono = cliente.Telefono;
                item.FlagClientePotencial = cliente.FlagClientePotencial;
                item.Direccion = cliente.Direccion;
                context.SaveChanges();
                return Ok(item);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
        [HttpGet]
        [Route("{ruta}")]
        public IHttpActionResult ClienteContadoList(string ruta)
        {
            try
            {
                var clientes = context.ClienteContado.Where(x => x.Ruta == ruta).OrderBy(x => x.Nombre).ToList();
                if (clientes.Count <= 0)
                {
                    return NotFound();
                }
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
