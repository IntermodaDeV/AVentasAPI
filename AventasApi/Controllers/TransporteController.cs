using AventasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AventasApi.Controllers
{ 
    [RoutePrefix("api/transporte")]
    public class TransporteController : ApiController
    {
        [Route("{empresa}/empresas")]
        public IEnumerable<EmpresaTransporte> GetEmpresasTransporte(string empresa)
        {
            if (empresa != null)
            {
                var respuesta = Proxy.Proxy.GetEmpresasTransporte(empresa);
                if (respuesta != null || respuesta.Count > 0)
                {
                    try
                    {
                        var resp = respuesta.Select(x => new EmpresaTransporte
                        {
                            CODE = x.CODE,
                            TXT = x.TXT,
                            MULTIPLO = x.MULTIPLO,
                            ACTIVE = (x.MULTIPLO>0)
                        });
                        return resp;
                    }
                    catch (System.Exception e)
                    {
                        return new List<EmpresaTransporte>();
                    }
                }
                return new List<EmpresaTransporte>();
            }
            return new List<EmpresaTransporte>();
        }

        [Route("{empresa}/preciocaja")]
        public IEnumerable<TransportePrecioCaja> GetPrecioCaja(string empresa)
        {
            if (empresa != null)
            {
                var respuesta = Proxy.Proxy.GetTransportePrecioCaja(empresa);
                if (respuesta != null || respuesta.Count > 0)
                {
                    try
                    {
                        var resp = respuesta.Select(x => new TransportePrecioCaja
                        {
                            CODE = x.CODE,
                            STATE = x.STATE,
                            UNITVALUEBOXES = Math.Round(x.UNITVALUEBOXES)
                        });
                        return resp;
                    }
                    catch (System.Exception e)
                    {
                        return new List<TransportePrecioCaja>();
                    }
                }
                return new List<TransportePrecioCaja>();
            }
            return new List<TransportePrecioCaja>();
        }

        [Route("{pais}/ComunidadAutonoma")]
        public IEnumerable<ComunidadAutonoma> GetComunidadAutonoma(string pais)
        {
            if (pais != null)
            {
                var respuesta = Proxy.Proxy.GetComunidadAutonoma(pais);
                if (respuesta != null || respuesta.Count > 0)
                {
                    try
                    {
                        var resp = respuesta.Select(x => new ComunidadAutonoma
                        {
                            STATEID = x.STATEID,
                            NAME = x.NAME,
                            COUNTRYREGIONID = x.COUNTRYREGIONID
                        });
                        return resp;
                    }
                    catch (System.Exception)
                    {
                        return new List<ComunidadAutonoma>();
                    }
                }
                return new List<ComunidadAutonoma>();
            }
            return new List<ComunidadAutonoma>();
        }
    }
}
