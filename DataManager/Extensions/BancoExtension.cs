using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class BancoCRMApiModelExtension
    {
        public static Bancos CreandoBanco(this BancoApiModel banco)
        {
            Bancos nuevoBanco = new Bancos()
            {
                EmpresaId = banco.COMPANY_CODE,
                NombreBanco = banco.CODE,
                Descripcion = banco.DESCRIPTION,
            };
            return nuevoBanco;
        }
    }
}
