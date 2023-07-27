using AventasApi.Models.ViewModels;
using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/linea")]
    public class LineaController : ApiController
    {
        [HttpGet]
        [Route("getLineasAsignadas/{IdUsuario}")]
        public async Task<IHttpActionResult> GetLineasAsignadas(int IdUsuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {

                    var LineasAsignadas = ctx.UsuarioLinea.Where(x => x.UsuarioId == IdUsuario && x.Asignada == true).Select(x => x.IdLinea).ToList();

                    var ListaEmpresas = await ctx.MaestroLinea.Where(e => LineasAsignadas.Contains(e.IdLinea) && e.Visible == true).Select(e => new UsuarioLineaViewModel
                    {
                        Id = e.IdLinea,
                        Nombre = e.IdLinea
                    }).ToListAsync();

                    return Ok(ListaEmpresas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("getLineasNoAsignadas/{IdUsuario}")]
        public async Task<IHttpActionResult> GetLineasNoAsignadas(int IdUsuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var LineasAsignadas = ctx.UsuarioLinea.Where(x => x.UsuarioId == IdUsuario && x.Asignada == true).Select(x => x.IdLinea).ToList();

                    var ListaEmpresas = await ctx.MaestroLinea.Where(e => !LineasAsignadas.Contains(e.IdLinea) && e.Visible == true).Select(e => new UsuarioLineaViewModel
                    {
                        Id = e.IdLinea,
                        Nombre = e.IdLinea
                    }).ToListAsync();

                    return Ok(ListaEmpresas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("AsignarLinea/{IdLinea}/{UsuarioId}/{usuario}")]
        public async Task<IHttpActionResult> AsignarEmpresa(string IdLinea, int UsuarioId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var UsuarioLinea = await ctx.UsuarioLinea.FirstOrDefaultAsync(x => x.UsuarioId == UsuarioId && x.IdLinea == IdLinea);

                    if (UsuarioLinea != null)
                    {
                        UsuarioLinea.Asignada = true;
                        UsuarioLinea.ModifiedBy = usuario;
                        UsuarioLinea.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var UsuariosLinea = new UsuarioLinea()
                        {
                            IdLinea = IdLinea,
                            UsuarioId = UsuarioId,
                            Asignada = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.UsuarioLinea.Add(UsuariosLinea);
                    }

                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }



        [HttpPost]
        [Route("RemoverLinea/{IdLinea}/{UsuarioId}/{usuario}")]
        public async Task<IHttpActionResult> RemoverEmpresa(string IdLinea, int UsuarioId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var UsuarioLinea = await ctx.UsuarioLinea.FirstOrDefaultAsync(x => x.IdLinea == IdLinea && x.UsuarioId == UsuarioId);

                    if (UsuarioLinea == null)
                    {
                        return BadRequest("La funcion no tiene asignada esta funcion");
                    }

                    UsuarioLinea.Asignada = false;
                    UsuarioLinea.ModifiedBy = usuario;
                    UsuarioLinea.ModifiedDate = DateTime.Now;
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
