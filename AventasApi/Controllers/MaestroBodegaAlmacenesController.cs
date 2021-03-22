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
                    List<BodegaAlmacenesViewModel> bodegaAlmacenes = await  db.MaestroBodegaAlmacenes.Select(ma => new BodegaAlmacenesViewModel
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
        [Route("~/api/GrupoOpcioness")]
        public async Task<IHttpActionResult> ObtenerGrupoOpciones()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var GrupoOpciones = await ctx.GrupoOpciones.Select(x => new
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Status = x.Status
                    }).ToListAsync();
                    return Ok(GrupoOpciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}