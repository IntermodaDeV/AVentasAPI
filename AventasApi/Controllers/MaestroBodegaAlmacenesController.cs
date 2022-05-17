using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class ModificarAlmacen
    {
        public int id { get; set; }
        public string etiqueta { get; set; }
        public bool estatus { get; set; }
        public string usuario { get; set; }
    }
    public class MaestroBodegaAlmacenesController : ApiController
    {

        [HttpGet]
        [Route("~/api/MaestroBodegaAlmacenes")]
        public async Task<IHttpActionResult> Obtener()
        {
            try
            {
                using (var db = new AVentasEntities())
                {
                    var SitioPrincipal = db.Configuraciones.Where(s => s.CodigoConfiguracion == "SitioPrincipal").Select(s => s.Valor).FirstOrDefault();
                    var BodegaPrincipal = db.Configuraciones.Where(s => s.CodigoConfiguracion == "BodegaPrincipal").Select(s => s.Valor).FirstOrDefault();
                    List<BodegaAlmacenesViewModel> bodegaAlmacenes = await  db.MaestroBodegaAlmacenes.Where(x=>x.Estatus==true).Select(ma => new BodegaAlmacenesViewModel
                    {
                        AlmacenId = ma.AlmacenId,
                        Almacen = ma.Almacen,
                        Nombre = ma.Nombre,
                        Etiqueta = ma.Etiqueta,
                        EmpresaId = ma.EmpresaId,
                        Estatus = ma.Estatus,
                        SitioId = ma.SitioId,
                        CodigoSitio = ma.MaestroBodegaSitios.Sitio,
                        BodegaPrincipal = SitioPrincipal == ma.MaestroBodegaSitios.Sitio && BodegaPrincipal == ma.Almacen
                    }).ToListAsync();

                    return Ok(bodegaAlmacenes);
                }
                    
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/devolucion/almacenes/{empresa}")]
        public async Task<IHttpActionResult> ObtenerAlmacenesDevolucion(string empresa)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var almacenes = await ctx.MaestroBodegaAlmacenes.Where(x => x.EmpresaId == empresa && x.ActivoDevolucion == true).Select(x => new {
                        Id = x.AlmacenId,
                        Sitio = x.MaestroBodegaSitios.Nombre,
                        Almacen = x.Almacen,
                        Nombre = x.Nombre,
                        Empresa = x.EmpresaId,
                        Etiqueta = x.Etiqueta
                    }).ToListAsync();
                    return Ok(almacenes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/almacenes")]
        public async Task<IHttpActionResult> ObtenerAlmacenes()
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var almacenes = await ctx.MaestroBodegaAlmacenes.Include(x=>x.MaestroBodegaSitios).Select(x => new { 
                        Id=x.AlmacenId,Sitio=x.MaestroBodegaSitios.Nombre,Almacen=x.Almacen,Nombre=x.Nombre,Empresa=x.EmpresaId,Etiqueta=x.Etiqueta,Estatus=x.Estatus
                    }).ToListAsync();
                    return Ok(almacenes);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/almacenes/modificar/estado/{id}")]
        public async Task<IHttpActionResult> ModificarEstadoAlmacen(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var almacen = await ctx.MaestroBodegaAlmacenes.FindAsync(id);

                    if (almacen == null)
                    {
                        return BadRequest("El almacen no existe");
                    }

                    almacen.Estatus = !almacen.Estatus;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/almacenes/modificar")]
        public async Task<IHttpActionResult> ModificarAlmacen([FromBody]ModificarAlmacen almacen)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var almacenBD = await ctx.MaestroBodegaAlmacenes.FindAsync(almacen.id);

                    if (almacenBD == null)
                    {
                        return BadRequest("No existe el almacen");
                    }

                    almacenBD.Etiqueta = almacen.etiqueta;
                    almacenBD.Estatus = almacen.estatus;
                    almacenBD.ModificadoPor = almacen.usuario;
                    almacenBD.FechaModificacion = DateTime.Now;

                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}