using DBData.Database;
using AventasApi.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class MonedaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        [Route("api/Moneda/{empresa}")]
        public async Task<IHttpActionResult> GetMonedas(string Empresa)
        {
            var MonedaXEmpresa = context.MonedasxEmpresa.Where(m => m.EmpresaId == Empresa).Select(m => m.IdMoneda).ToList();
            var monedas = context.MaestroMoneda.Where(m => MonedaXEmpresa.Contains(m.IdMoneda)).Select(mon => new MonedaViewModel
            {
                IdMoneda = mon.IdMoneda,
                Moneda = mon.Moneda,
                Abreviacion=mon.Abreviacion
            }).ToList();
            return Ok(monedas);
        }
    }
}
