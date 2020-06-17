using DBData.Database;
using Proxy;

namespace DataManager.Extensions
{

    public static class GrupoImpuestoArticuloExtension
    {
        public static GrupoImpuestoArticulo ToImpuestoArticulos(this IMObtenerGrupoImpuestoArticulos_Result GrupoArticulos)
        {

            GrupoImpuestoArticulo ArticuloAGuardar = new GrupoImpuestoArticulo
            {
                GrupoProducto = GrupoArticulos.TAXITEMGROUP,
                GrupoImpuesto = GrupoArticulos.TAXCODE,
                Porcentaje = GrupoArticulos.PORCENTAJE,
                Empresa = GrupoArticulos.DATAAREAID
            };
            return ArticuloAGuardar;
        }

    }

}