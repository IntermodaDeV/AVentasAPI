using AventasApi.Models;
using DBData.Database;
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
    }
}
