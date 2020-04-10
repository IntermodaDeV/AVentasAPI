using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class CuentasBancariasCRMApiModelExtension
    {
        public static CuentasBancarias CreandoCuentaBancaria(this CuentasBancariasCRMApiModel cuenta)
        {
            CuentasBancarias nuevaCuenta = new CuentasBancarias()
            {
                NombreBanco = cuenta?.CODE,
                NumeroCuenta = cuenta?.ACCOUNT_NUM,
                Descripcion = cuenta?.DESCRIPTION,
                GrupoBanco = cuenta?.BANK_GROUP,
                IdBanco = null,
                IdMoneda = cuenta?.CURRENCY,
                EmpresaId = cuenta?.COMPANY_CODE,
            };
            return nuevaCuenta;
        }
    }
}
