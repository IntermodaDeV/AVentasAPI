using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using System.Data.Entity;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/empresa")]
    public class EmpresaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [Route("Empresas")]
        public IEnumerable<EmpresaModel> GetEmpresas()
        {

            try
            {
                var empresas = context.Empresa.ToList();

                if(empresas.Count <= 0)
                {
                    return new List<EmpresaModel>();
                }
                var resp = empresas.Select(x => new EmpresaModel
                {
                    ADDRESS = x.Direccion,
                    COMPANY_CODE = x.EmpresaId,
                    NAME = x.NombreEmpresa,
                    NIFCIF = x.RegistroTributario,
                    FISCAL_DOCUMENT = x.DocumentoFiscal
                });

                return resp;
            }
            catch (System.Exception)
            {
                return new List<EmpresaModel>();
            }
          
        }

        [HttpGet]
        [Route("EmpresasAsignadas/{Id}")]
        public async Task<IHttpActionResult> GetEmpresaAsignada(int Id)
        {
            try
            {
                using (var emp = new AVentasEntities())
                {
                    var EmpresasAsignadas = emp.Usuarios_Empresas.Where(x => x.UsuarioId == Id && x.Status == true).Select(x => x.EmpresaId).ToList();

                    var ListaEmpresas = await emp.Empresa.Where(e => EmpresasAsignadas.Contains(e.EmpresaId)).Select(e => new EmpresaViewModel
                    {
                        Id = e.EmpresaId,
                        Nombre = e.EmpresaId
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
        [Route("EmpresasNoAsignadas/{Id}")]
        public async Task<IHttpActionResult> GetEmpresasNoAsignadas(int Id)
        {
            try
            {
                using (var emp = new AVentasEntities())
                {
                    var EmpresasAsignadas = emp.Usuarios_Empresas.Where(x => x.UsuarioId == Id && x.Status == true).Select(x => x.EmpresaId).ToList();

                    var ListaEmpresas = await emp.Empresa.Where(e => !EmpresasAsignadas.Contains(e.EmpresaId)).Select(e => new EmpresaViewModel
                    {
                        Id = e.EmpresaId,
                        Nombre = e.EmpresaId
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
        [Route("AsignarEmpresa/{EmpresaId}/{UsuarioId}/{usuario}")]
        public async Task<IHttpActionResult> AsignarEmpresa(string EmpresaId, int UsuarioId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var UsuarioEmpresa = await ctx.Usuarios_Empresas.FirstOrDefaultAsync(x => x.UsuarioId == UsuarioId && x.EmpresaId == EmpresaId);

                    if (UsuarioEmpresa != null)
                    {
                        UsuarioEmpresa.Status = true;
                        UsuarioEmpresa.ModifiedBy = usuario;
                        UsuarioEmpresa.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        var UsuariosEmpresas = new Usuarios_Empresas()
                        {
                            EmpresaId = EmpresaId,
                            UsuarioId = UsuarioId,
                            Status = true,
                            CreatedBy = usuario,
                            CreatedDate = DateTime.Now
                        };
                        ctx.Usuarios_Empresas.Add(UsuariosEmpresas);
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
        [Route("RemoverEmpresa/{EmpresaId}/{UsuarioId}/{usuario}")]
        public async Task<IHttpActionResult> RemoverEmpresa(string EmpresaId, int UsuarioId, string usuario)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var UsuarioEmpresa = await ctx.Usuarios_Empresas.FirstOrDefaultAsync(x => x.EmpresaId == EmpresaId && x.UsuarioId == UsuarioId);

                    if (UsuarioEmpresa == null)
                    {
                        return BadRequest("La funcion no tiene asignada esta funcion");
                    }

                    UsuarioEmpresa.Status = false;
                    UsuarioEmpresa.ModifiedBy = usuario;
                    UsuarioEmpresa.ModifiedDate = DateTime.Now;
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
