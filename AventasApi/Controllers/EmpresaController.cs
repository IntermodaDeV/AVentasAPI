using AventasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/empresa")]
    public class EmpresaController : ApiController
    {
        [Route("Empresas")]
        public IEnumerable<Empresa> GetEmpresas()
        {
            var respuesta = Proxy.Proxy.GetEmpresas();
            if (respuesta != null || respuesta.Count > 0)
            {
                try
                {
                    var resp = respuesta.Select(x => new Empresa
                    {
                        ADDRESS = x.ADDRESS,
                        COMPANY_CODE = x.COMPANY_CODE,
                        NAME = x.NAME,
                        NIFCIF = x.NIFCIF,
                        FISCAL_DOCUMENT = x.FISCAL_DOCUMENT
                    });

                    return resp;
                }
                catch (System.Exception)
                {
                    return new List<Empresa>();
                }
            }

            return new List<Empresa>();
        }
    }
}
