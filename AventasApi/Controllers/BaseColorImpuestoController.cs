using DBData.Database;
using System;
using System.Linq;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/basecolorimpuesto")]
    public class BaseColorImpuestoController : ApiController
    {
        [HttpGet]
        [Route("admin")]
        public IHttpActionResult ObtenerAdmin()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var elementos = ctx.BaseColorImpuesto.Select(x => new { id = x.id, codigobase = x.codigobase, color = x.color, impuesto = x.porcentajeImpuesto, empresa = x.empresa.ToUpper(), estado = x.activo, codigoImpuesto = x.codigoimpuesto }).ToList();
                    return Ok(elementos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("colores")]
        public IHttpActionResult ObtenerColores()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var elementos = ctx.Colores.Select(x => new { codigo = x.CodigoColor, nombre = x.Color }).ToList();
                    return Ok(elementos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("bases")]
        public IHttpActionResult ObtenerBases()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var elementos = ctx.IMObtenerBasesProductos().ToList();
                    return Ok(elementos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        public IHttpActionResult ObtenerActivos()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var elementos = ctx.BaseColorImpuesto.Where(x => x.activo == true).Select(x => new { id = x.id, codigobase = x.codigobase, color = x.color, impuesto = (x.porcentajeImpuesto / 100), empresa = x.empresa.ToUpper(), codigoImpuesto = x.codigoimpuesto }).ToList();
                    return Ok(elementos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        public IHttpActionResult CrearCombinacion([FromBody] CombinacionPost body)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var existeCombinacion = ctx.BaseColorImpuesto.FirstOrDefault(x => x.codigobase.ToUpper() == body.CodigoBase.ToUpper() && x.color.ToUpper() == body.Color.ToUpper() && x.empresa.ToUpper() == body.Empresa.ToUpper() && x.codigoimpuesto.ToUpper() == body.CodigoImpuesto.ToUpper());
                    if (existeCombinacion != null)
                    {
                        return BadRequest("Ya existe una combinación.");
                    }

                    ctx.BaseColorImpuesto.Add(new BaseColorImpuesto { codigobase = body.CodigoBase, color = body.Color, empresa = body.Empresa, porcentajeImpuesto = body.Impuesto, codigoimpuesto = body.CodigoImpuesto, usuario = body.Usuario, fechaCreacion = DateTime.Now, fechaModificacion = DateTime.Now, activo = true });
                    ctx.SaveChanges();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult CrearCombinacion(int id, [FromBody] CombinacionPost body)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var combinacion = ctx.BaseColorImpuesto.Find(id);
                    if (combinacion == null)
                    {
                        return BadRequest("No existe la combinación");
                    }

                    var existeCombinacion = ctx.BaseColorImpuesto.FirstOrDefault(x => x.codigobase.ToUpper() == body.CodigoBase.ToUpper() && x.color.ToUpper() == body.Color.ToUpper() && x.empresa.ToUpper() == body.Empresa.ToUpper() && x.porcentajeImpuesto == body.Impuesto && x.codigoimpuesto == body.CodigoImpuesto.ToUpper());
                    if (existeCombinacion != null)
                    {
                        return BadRequest("Ya existe una combinación.");
                    }

                    combinacion.codigobase = body.CodigoBase;
                    combinacion.color = body.Color;
                    combinacion.porcentajeImpuesto = body.Impuesto;
                    combinacion.codigoimpuesto = body.CodigoImpuesto;
                    combinacion.empresa = body.Empresa;
                    combinacion.fechaModificacion = DateTime.Now;
                    combinacion.usuario = body.Usuario;
                    ctx.SaveChanges();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPatch]
        [Route("estado/{id}")]
        public IHttpActionResult DesactivarCombinacion(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var combinacion = ctx.BaseColorImpuesto.Find(id);
                    if (combinacion == null)
                    {
                        return NotFound();
                    }

                    combinacion.activo = !combinacion.activo;
                    ctx.SaveChanges();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("impuestos")]
        public IHttpActionResult ObtenerImpuestos()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var impuestos = ctx.GrupoImpuestoArticulo.Select(x => new { impuesto = x.GrupoProducto, empresa = x.Empresa }).Distinct().ToList();
                    return Ok(impuestos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }

    public class CombinacionPost
    {
        public string CodigoBase { get; set; }
        public string Color { get; set; }
        public string CodigoImpuesto { get; set; }
        public decimal Impuesto { get; set; }
        public string Empresa { get; set; }
        public string Usuario { get; set; }
    }
}
