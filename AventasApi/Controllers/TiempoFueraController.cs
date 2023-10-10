using DBData.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/tiemposfuera")]
    public class TiemposFueraController : ApiController
    {
        [HttpPost]
        [Route("motivotiempofuera")]
        public async Task<IHttpActionResult> CrearMotivoFueraAgenda([FromBody] CreateMotivoFueraAgendaModel model)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    MotivoTiempoFuera motivo = new MotivoTiempoFuera() { activo = true, descripcion = model.motivo };
                    ctx.MotivoTiempoFuera.Add(motivo);
                    await ctx.SaveChangesAsync();

                    var motivoDto = new { id = motivo.id, descripcion = motivo.descripcion, activo = motivo.activo };

                    return Ok(motivoDto);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        [Route("motivotiempofuera/estado/{id}")]
        public async Task<IHttpActionResult> CambiarEstadoMotivoTiempoFuera(int id)
        {
            try
            {
                using (var context = new AVentasEntities())
                {
                    var motivoTiempoFuera = await context.MotivoTiempoFuera.FirstOrDefaultAsync(x => x.id == id);
                    if (motivoTiempoFuera == null)
                    {
                        return NotFound();
                    }

                    motivoTiempoFuera.activo = !motivoTiempoFuera.activo;
                    await context.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        [Route("asesor")]
        public async Task<IHttpActionResult> CrearTiempoFueraAgenda([FromBody] CreateTiempoFuera model)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var motivoEntity = await ctx.MotivoTiempoFuera.FirstOrDefaultAsync(x => x.id == model.motivoTiempoFueraId);
                    if (motivoEntity == null)
                    {
                        return NotFound();
                    }

                    var tiempoFuera = new TiempoFueraAgenda() { motivoTiempoFueraId = model.motivoTiempoFueraId, latitudEntrada = model.latitudEntrada, longitudEntrada = model.longitudEntrada, codigoAsesor = model.codigoAsesor, horaEntrada = DateTime.Now, descripcion = model.descripcion };
                    ctx.TiempoFueraAgenda.Add(tiempoFuera);
                    await ctx.SaveChangesAsync();

                    var tiempoFueraDto = new { id = tiempoFuera.id, motivo = motivoEntity.descripcion, horaEntrada = tiempoFuera.horaEntrada, horaSalida = tiempoFuera.horaSalida };
                    return Ok(tiempoFueraDto);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPut]
        [Route("asesor")]
        public async Task<IHttpActionResult> CompletarTiempoFueraAgenda([FromBody] CompletarTiempoFuera model)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var tiempoFueraEntity = await ctx.TiempoFueraAgenda.Include(x => x.MotivoTiempoFuera).FirstOrDefaultAsync(x => x.id == model.tiempoFueraId);
                    if (tiempoFueraEntity == null)
                    {
                        return NotFound();
                    }


                    tiempoFueraEntity.horaSalida = DateTime.Now;
                    tiempoFueraEntity.latitudSalida = model.latitudSalida;
                    tiempoFueraEntity.longitudSalida = model.longitudSalida;

                    await ctx.SaveChangesAsync();

                    var tiempoFueraDto = new { id = tiempoFueraEntity.id, motivo = tiempoFueraEntity.MotivoTiempoFuera.descripcion, horaEntrada = tiempoFueraEntity.horaEntrada, horaSalida = tiempoFueraEntity.horaSalida };
                    return Ok(tiempoFueraDto);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("diario/{asesor}")]
        public IHttpActionResult GetTiemposFueraAsesorDiario(string asesor)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var tiempos = ctx.SPObtenerTiemposFueraDelDia(asesor).ToList();
                    return Ok(tiempos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("motivotiempofuera/admin")]
        public async Task<IHttpActionResult> GetMotivoFueraAgendaAdmin()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var motivos = await ctx.MotivoTiempoFuera.Select(x => new { id = x.id, descripcion = x.descripcion, activo = x.activo }).ToListAsync();
                    return Ok(motivos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("motivotiempofuera/asesor")]
        public async Task<IHttpActionResult> GetMotivoFueraAgendaAsesor()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var motivos = await ctx.MotivoTiempoFuera.Where(x => x.activo).Select(x => new { id = x.id, descripcion = x.descripcion, activo = x.activo }).ToListAsync();
                    return Ok(motivos);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }

    public class CreateMotivoFueraAgendaModel
    {
        public string motivo { get; set; }
    }

    public class CreateTiempoFuera
    {
        public int motivoTiempoFueraId { get; set; }
        public string codigoAsesor { get; set; }
        public Nullable<decimal> latitudEntrada { get; set; }
        public Nullable<decimal> longitudEntrada { get; set; }
        public string descripcion { get; set; }
    }

    public class CompletarTiempoFuera
    {
        public int tiempoFueraId { get; set; }
        public Nullable<decimal> latitudSalida { get; set; }
        public Nullable<decimal> longitudSalida { get; set; }
    }
}
