using DBData.Database;
using ExternalApiData.Models.ApiModels;

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
