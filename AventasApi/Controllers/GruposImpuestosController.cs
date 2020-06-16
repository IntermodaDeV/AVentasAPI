using AventasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;


namespace AventasApi.Controllers
{
    [RoutePrefix("api/gruposimpuestos")]
    public class GruposImpuestosController : ApiController
    {
        [Route("{empresa}/Clientes")]
        public IEnumerable<GruposImpuestosClientes> GetGruposImpuestosClientes(string empresa)
        {
            if (empresa != null)
            {
                var respuesta = Proxy.Proxy.GetGrupoImpuestoClientes(empresa);
                if (respuesta != null || respuesta.Count > 0)
                {
                    try
                    {
                        var resp = respuesta.Select(x => new GruposImpuestosClientes
                        {
                            TAXGROUP = x.TAXGROUP,
                            TAXCODE = x.TAXCODE,
                            PORCENTAJE = x.PORCENTAJE.ToString()
                        });
                        return resp;
                    }
                    catch (System.Exception e)
                    {
                        return new List<GruposImpuestosClientes>();
                    }
                }
                return new List<GruposImpuestosClientes>();
            }
            return new List<GruposImpuestosClientes>();
        }
        [Route("{empresa}/Articulos")]
        public IEnumerable<GruposImpuestosArticulos> GetGruposImpuestosArticulos(string empresa)
        {
            if (empresa != null)
            {
                var respuesta = Proxy.Proxy.GetGrupoImpuestoArticulos(empresa);
                if (respuesta != null || respuesta.Count > 0)
                {
                    try
                    {
                        var resp = respuesta.Select(x => new GruposImpuestosArticulos
                        {
                            TAXITEMGROUP = x.TAXITEMGROUP,
                            TAXCODE = x.TAXCODE,
                            PORCENTAJE = x.PORCENTAJE.ToString()
                        });
                        return resp;
                    }
                    catch (System.Exception e)
                    {
                        return new List<GruposImpuestosArticulos>();
                    }
                }
                return new List<GruposImpuestosArticulos>();
            }
            return new List<GruposImpuestosArticulos>();
        }
    }
}