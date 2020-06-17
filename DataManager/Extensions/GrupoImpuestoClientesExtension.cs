using DBData.Database;
using Proxy;

namespace DataManager.Extensions
{

    public static class GrupoImpuestoClientesExtension
    {
        public static GrupoImpuestoCliente ToImpuestoClientes(this IMObtenerGrupoImpuestoClientes_Result GrupoImpClientes)
        {

            GrupoImpuestoCliente GrupoClientesAGuardar = new GrupoImpuestoCliente
            {
                GrupoCliente = GrupoImpClientes.TAXGROUP,
                GrupoImpuesto = GrupoImpClientes.TAXCODE,
                Porcentaje = GrupoImpClientes.PORCENTAJE,
                Empresa = GrupoImpClientes.DATAAREAID
            };
            return GrupoClientesAGuardar;
        }
    }

}