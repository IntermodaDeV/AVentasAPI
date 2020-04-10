using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class EmpresasCRMApiModelExtension
    {
        public static Empresa CreandoEmpresa(this EmpresasCRMApiModel empresa)
        {
            Empresa nuevaEmpresa = new Empresa()
            {
                EmpresaId = empresa.COMPANY_CODE,
                NombreEmpresa = empresa.NAME,
                Direccion = empresa.ADDRESS,
                RegistroTributario = empresa.NIFCIF,
                Revision = null
            };
            return nuevaEmpresa;
        }
    }
}
