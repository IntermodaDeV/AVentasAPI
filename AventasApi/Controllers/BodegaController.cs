using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class PostPaquete
    {
        public string coleccion { get; set; }
        public string empresa { get; set; }
        public int sitio { get; set; }
        public string almacen { get; set; }
        public string usuario { get; set; }
    }

    [RoutePrefix("api/bodega")]
    public class BodegaController : ApiController
    {
        [HttpGet]
        [Route("almacen")]
        public async Task<IHttpActionResult> ObtenerAlmacenes()
        {
            try
            {
                using(AVentasEntities ctx=new AVentasEntities())
                {
                    var bodegas = await ctx.MaestroBodegaAlmacenes.Where(x=>x.Estatus==true).Select(x => new {Id=x.AlmacenId,Nombre=x.Nombre,Almacen=x.Almacen,SitioId=x.SitioId,Empresa=x.EmpresaId}).ToListAsync();
                    return Ok(bodegas);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("sitio")]
        public async Task<IHttpActionResult> ObtenerSitios()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var bodegas = await ctx.MaestroBodegaSitios.Where(x=>x.Estatus==true).Select(x => new { Id = x.SitioId, Sitio = x.Sitio, Nombre = x.Nombre, Empresa = x.EmpresaId }).ToListAsync();
                    return Ok(bodegas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("paquetes")]
        public async Task<IHttpActionResult> ObtenerPaquetesEspcificos()
        {
            try
            {
                using(AVentasEntities ctx=new AVentasEntities())
                {
                    var paquetesEspecificos = await ctx.PaqueteBodegaEspecifico.Include(x => x.Colecciones).Include(x => x.MaestroBodegaAlmacenes).Include(x => x.MaestroBodegaSitios).Select(x => new
                    {
                        Id=x.Id,
                        Codigo=x.Colecciones.CodigoColeccion,
                        Coleccion=x.Colecciones.Nombre,
                        Almacen=x.MaestroBodegaAlmacenes.Nombre,
                        Sitio=x.MaestroBodegaSitios.Nombre,
                        Estado=x.Estado,
                        Empresa=x.EmpresaId
                    }).ToListAsync();
                    return Ok(paquetesEspecificos);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> CrearBodegaEspecifico([FromBody] PostPaquete postPaquete)
        {
            try
            {
                using(AVentasEntities ctx=new AVentasEntities())
                {
                    var coleccion = await ctx.Colecciones.FirstOrDefaultAsync(x => x.CodigoColeccion == postPaquete.coleccion && x.EmpresaId==postPaquete.empresa);                  
                    var sitio = await ctx.MaestroBodegaSitios.FirstOrDefaultAsync(x => x.SitioId == postPaquete.sitio);
                    var almacen = await ctx.MaestroBodegaAlmacenes.FirstOrDefaultAsync(x => x.Almacen == postPaquete.almacen);

                    var paqueteExistente = await ctx.PaqueteBodegaEspecifico.FirstOrDefaultAsync(x=>x.ColeccionId==coleccion.IdColeccion && x.EmpresaId==postPaquete.empresa && x.Almacen==postPaquete.almacen && x.Sitio==sitio.Sitio);

                    if (paqueteExistente != null)
                    {
                        return BadRequest("El paquete ya ha sido registrado anteriormente");
                    }

                    PaqueteBodegaEspecifico nuevoPaquete = new PaqueteBodegaEspecifico()
                    {
                        ColeccionId = coleccion.IdColeccion,
                        EmpresaId = postPaquete.empresa,
                        CreadoPor = postPaquete.usuario,
                        FechaCreacion = DateTime.Now,
                        Estado = true,
                        Almacen = postPaquete.almacen,
                        AlmacenId = almacen.AlmacenId,
                        Sitio = sitio.Sitio,
                        SitioId = sitio.SitioId
                    };

                    ctx.PaqueteBodegaEspecifico.Add(nuevoPaquete);
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("modificar/{id}/{usuario}")]
        public async Task<IHttpActionResult> ModificarBodegaEspecifico(int id,string usuario)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var paqueteEspecifico = await ctx.PaqueteBodegaEspecifico.FindAsync(id);
                    paqueteEspecifico.Estado = !paqueteEspecifico.Estado;
                    paqueteEspecifico.ModificadoPor = usuario;
                    paqueteEspecifico.FechaModificacion = DateTime.Now;

                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
