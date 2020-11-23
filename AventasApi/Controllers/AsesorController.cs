using AventasApi.Filters;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
//using AventasApi.GestorData;

//using DBData.Database;
using AventasApi.Models.ViewModels;
using System.Data.Entity;

public class AsesorController : ApiController
{
    AVentasEntities context = new AVentasEntities();


    [HttpGet]
    public async Task<IHttpActionResult> GetAsesores()
    {
        return Ok(context.Asesores.Select(ase => new AsesorViewModel { CodigoAsesor = ase.CodigoAsesor, Nombre = ase.Nombre }).ToList());
    }

    [HttpGet]
    [Route("api/AsesoresAsignados/{Id}")]
    public async Task<IHttpActionResult> ObtenerAsesoresxUsuarios(int Id)
    {
        try
        {
            using (var ase = new AVentasEntities())
            {
                var Asesores = ase.Usuarios_Asesores.Where(x => x.UsuarioId == Id && x.Status == true).Select(x => x.CodigoAsesor).ToList();

                var AsesoresAsignados = await ase.Asesores.Where(e => Asesores.Contains(e.CodigoAsesor)).Select(e => new AsesoresViewModel
                {
                    Id = e.CodigoAsesor,
                    Nombre = e.Nombre
                }).ToListAsync();

                return Ok(AsesoresAsignados);
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToString());
        }
    }

    [HttpGet]
    [Route("api/AsesoresNoAsignados/{Id}")]
    public async Task<IHttpActionResult> ObtenerAsesoresNoAsignados(int Id)
    {
        try
        {
            using (var ase = new AVentasEntities())
            {

                var Asesores = ase.Usuarios_Asesores.Where(x => x.UsuarioId == Id && x.Status == true).Select(x => x.CodigoAsesor).ToList();

                var EmpresasPermitidas = ase.Usuarios_Empresas.Where(e => e.UsuarioId == Id && e.Status == true).Select(e => e.EmpresaId).ToList();

                var AsesoresNoAsignados = await ase.Asesores.Where(e => !Asesores.Contains(e.CodigoAsesor) && EmpresasPermitidas.Contains(e.EmpresaId)).Select(e => new AsesoresViewModel
                {
                    Id = e.CodigoAsesor,
                    Nombre = e.Nombre
                }).ToListAsync();

                return Ok(AsesoresNoAsignados);
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToString());
        }
    }

    [HttpPost]
    [Route("api/AsignarAsesores/{CodigoAsesor}/{UsuarioId}/{Usuario}")]
    public async Task<IHttpActionResult> AsignarAsesores(string CodigoAsesor, int UsuarioId, string Usuario)
    {
        try
        {
            using (var ctx = new AVentasEntities())
            {
                var UsuarioAsesor = await ctx.Usuarios_Asesores.FirstOrDefaultAsync(x => x.UsuarioId == UsuarioId && x.CodigoAsesor == CodigoAsesor);

                if (UsuarioAsesor != null)
                {
                    UsuarioAsesor.Status = true;
                    UsuarioAsesor.ModifiedBy = Usuario;
                    UsuarioAsesor.ModifiedDate = DateTime.Now;
                }
                else
                {
                    var UsuariosAsesores = new Usuarios_Asesores()
                    {
                        CodigoAsesor = CodigoAsesor,
                        UsuarioId = UsuarioId,
                        Status = true,
                        CreatedBy = Usuario,
                        CreatedDate = DateTime.Now
                    };
                    ctx.Usuarios_Asesores.Add(UsuariosAsesores);
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
    [Route("api/RemoverAsesor/{CodigoAsesor}/{UsuarioId}/{Usuario}")]
    public async Task<IHttpActionResult> RemoverAsesores(string CodigoAsesor, int UsuarioId, string Usuario)
    {
        try
        {
            using (var ctx = new AVentasEntities())
            {
                var UsuarioAsesor = await ctx.Usuarios_Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor == CodigoAsesor && x.UsuarioId == UsuarioId);

                if (UsuarioAsesor == null)
                {
                    return BadRequest("El usuario no tiene asignado este asesor");
                }

                UsuarioAsesor.Status = false;
                UsuarioAsesor.ModifiedBy = Usuario;
                UsuarioAsesor.ModifiedDate = DateTime.Now;
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
public class AsesorViewModel
{
    public string CodigoAsesor { get; set; }
    public string Nombre { get; set; }
}

public class AsesoresViewModel
{
    public string Id { get; set; }
    public string Nombre { get; set; }
}
