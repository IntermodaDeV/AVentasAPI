using AventasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DBData.Database;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/gruposimpuestos")]
    public class GruposImpuestosController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpGet]
        [Route("{empresa}/Clientes")]
        public IHttpActionResult GruposImpuestosClientes(string empresa)
        {
            try
                {
                    var clientes = context.GrupoImpuestoCliente.Where(x => x.Empresa == empresa && x.Activo==true).OrderBy(x => x.GrupoCliente).ToList();
                    if (clientes.Count <= 0)
                    {
                        return NotFound();
                    }

                    var grupos = clientes.Select(x => new ClienteImpuestoModel() { 
                        GRUPO = x.GrupoCliente.ToUpper(),
                        IMPUESTO = (x.Porcentaje/100) }
                    ).ToList();

                    return Ok(grupos);
                }
                catch (Exception e)
                {
                    return BadRequest();
                }
        }
        [HttpGet]
        [Route("{empresa}/Articulos")]
        public IHttpActionResult GruposImpuestosArticulos(string empresa)
        {
            try
            {
                var ImpProductos = context.GrupoImpuestoArticulo.Where(x => x.Empresa == empresa && x.Activo==true).OrderBy(x => x.GrupoProducto).ToList();
                if (ImpProductos.Count <= 0)
                {
                    return NotFound();
                }

                var grupos = ImpProductos.Select(x => new ProductoImpuestoModel()
                {
                    GRUPO = x.GrupoProducto.ToUpper(),
                    IMPUESTO = (x.Porcentaje/100)
                }).ToList();

                return Ok(grupos);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}