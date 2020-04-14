using DBData.Database;
using ExternalApiData.ApiModels;

namespace DataManager.Extensions
{
    public static class EspecificacionPagosExtension
    {
        public static TiposdePagoDetalle CreandoPago(this EspecificacionPagosCRMApiModel tipo)
        {
            TiposdePagoDetalle nuevoTipo = new TiposdePagoDetalle()
            {
                Codigo = tipo.CODE,
                CodigoDetalle = tipo.SPEC_CODE,
                Descripcion = tipo.DESCRIPTION,              
                EmpresaId = tipo.COMPANY_CODE,
                IdTipoPago = null
            };
            return nuevoTipo;
        }
    }
}
