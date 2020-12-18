using DBData.Database;
using AventasApi.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Data.Entity;
using System.Collections.Generic;

namespace AventasApi.Controllers
{
    public class MonedaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        [Route("api/Moneda/{empresa}")]
        public async Task<IHttpActionResult> GetMonedas(string Empresa)
        {
            try
            {
                var MonedaXEmpresa = await context.MonedasxEmpresa.Where(m => m.EmpresaId == Empresa).Select(m => m.IdMoneda).ToListAsync();
                var monedas = await context.MaestroMoneda.Where(m => MonedaXEmpresa.Contains(m.IdMoneda)).Select(mon => new MonedaViewModel
                {
                    IdMoneda = mon.IdMoneda,
                    Moneda = mon.Moneda,
                    Abreviacion = mon.Abreviacion
                }).ToListAsync();
                return Ok(monedas);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("api/Moneda")]
        public async Task<IHttpActionResult> GetMonedasAbreviacion()
        {
            try
            {
                var monedas = await context.MaestroMoneda.Select(mon => new MonedaViewModel
                {
                    IdMoneda = mon.IdMoneda,
                    Moneda = mon.Moneda,
                    Abreviacion = mon.Abreviacion
                }).ToListAsync();
                return Ok(monedas);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("api/Moneda/monedas")]
        public async Task<IHttpActionResult> GetMonedas()
        {
            try
            {
                using(var ctx=new AVentasEntities())
                {
                    List<MonedaViewModel> listaMonedas = new List<MonedaViewModel>();
                    var empresas = await ctx.Empresa.Select(x => x.EmpresaId).ToListAsync();

                    foreach(var empresa in empresas)
                    {
                        var MonedaXEmpresa = await ctx.MonedasxEmpresa.Where(m => m.EmpresaId == empresa).Select(m => m.IdMoneda).ToListAsync();
                        var monedas = await ctx.MaestroMoneda.Where(m => MonedaXEmpresa.Contains(m.IdMoneda)).Select(mon => new MonedaViewModel
                        {
                            IdMoneda = mon.IdMoneda,
                            Moneda = mon.Moneda,
                            Abreviacion = mon.Abreviacion,
                            Empresa = empresa
                        }).ToListAsync();

                        listaMonedas.AddRange(monedas);
                    }
                    
                    return Ok(listaMonedas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
