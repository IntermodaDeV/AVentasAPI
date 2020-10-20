using AventasApi.Models;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<ComunidadAutonomaModel> GetComunidadAutonoma()
        {
            try
            {
                var empresas = context.ComunidadAutonoma.ToList();
                if (empresas.Count <= 0)
                {
                    return new List<ComunidadAutonomaModel>();
                }

                var grupos = empresas.Select(x => new ComunidadAutonomaModel()
                {
                    STATEID = x.STATEID,
                    NAME = x.NAME,
                    COUNTRYREGIONID = x.COUNTRYREGIONID
                }
                ).ToList();

                return grupos;
            }
            catch (Exception e)
            {
                return new List<ComunidadAutonomaModel>();
            }
        }
    }
}
