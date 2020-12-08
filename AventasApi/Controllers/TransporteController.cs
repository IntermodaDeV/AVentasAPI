using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{ 
    [RoutePrefix("api/transporte")]
    public class TransporteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [Route("{empresa}/empresas")]
        public IEnumerable<EmpresaTransporteModel> GetEmpresasTransporte(string empresa)
        {
            try
            {
                var empresas = context.EmpresaTransporte.Where(x => x.COMPANY.ToUpper() == empresa.ToUpper() && x.MULTIPLO>0).ToList();
                if (empresas.Count <= 0)
                {
                    return new List<EmpresaTransporteModel>();
                }

                var grupos = empresas.Select(x => new EmpresaTransporteModel()
                {
                    CODE = x.CODE,
                    TXT = x.TXT,
                    MULTIPLO = x.MULTIPLO.Value,
                    ACTIVE = (x.MULTIPLO > 0)
                }
                ).ToList();

                return grupos;
            }
            catch (Exception e)
            {
                return new List<EmpresaTransporteModel>();
            }
            
        }

        [Route("{empresa}/preciocaja")]
        public IEnumerable<TransportePrecioCajaModel> GetPrecioCaja(string empresa)
        {
            try
            {
                var empresas = context.TransportePrecioCaja.Where(x => x.COMPANY.ToUpper() == empresa.ToUpper()).ToList();
                if (empresas.Count <= 0)
                {
                    return new List<TransportePrecioCajaModel>();
                }

                var grupos = empresas.Select(x => new TransportePrecioCajaModel()
                {
                    CODE = x.CODE,
                    STATE = x.STATE,
                    UNITVALUEBOXES = Math.Round(x.UNITVALUEBOXES.Value)
                }
                ).ToList();

                return grupos;
            }
            catch (Exception e)
            {
                return new List<TransportePrecioCajaModel>();
            }
        }

        [Route("ComunidadAutonoma")]
        public async Task<IHttpActionResult> GetComunidadAutonoma()
        {
            try
            {
                using(var ctx= new AVentasEntities())
                {
                    var empresas =await ctx.ComunidadAutonoma.ToListAsync();
                    var grupos = empresas.Select(x => new ComunidadAutonomaModel()
                    {
                        STATEID = x.STATEID,
                        NAME = x.NAME,
                        COUNTRYREGIONID = x.COUNTRYREGIONID
                    }
                    ).ToList();

                    return Ok(grupos);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [Route("preciocaja")]
        public async Task<IHttpActionResult> GetPrecioCaja()
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    var empresas = await ctx.TransportePrecioCaja.ToListAsync();
                    var grupos = empresas.Select(x => new TransportePrecioCajaModel()
                    {
                        CODE = x.CODE,
                        STATE = x.STATE,
                        UNITVALUEBOXES = Math.Round(x.UNITVALUEBOXES.Value),
                        COMPANY=x.COMPANY.ToUpper()
                    }
                    ).ToList();

                    return Ok(grupos);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [Route("empresas")]
        public async Task<IHttpActionResult> GetEmpresasTransporte()
        {
            try
            {
                using(var ctx=new AVentasEntities())
                {
                    var empresas = await ctx.EmpresaTransporte.ToListAsync();
                    
                    var grupos = empresas.Select(x => new EmpresaTransporteModel()
                    {
                        CODE = x.CODE,
                        TXT = x.TXT,
                        MULTIPLO = x.MULTIPLO.Value,
                        ACTIVE = (x.MULTIPLO > 0),
                        COMPANY=x.COMPANY.ToUpper()
                    }
                    ).ToList();

                    return Ok(grupos);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }
    }
}
