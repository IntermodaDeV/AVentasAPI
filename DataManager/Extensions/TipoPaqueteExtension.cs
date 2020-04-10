using DBData.Database;
using ExternalApiData.Models.ApiModels;

namespace DataManager.Extensions
{
    public static class TipoPaqueteExtension
    {
        public static TiposdeColeccion CreandoTipoColeccion(this ColeccionesCRMApiViewModel coleccion)
        {
            TiposdeColeccion nuevoTipoColeccion = new TiposdeColeccion()
            {
                ColeccionTipo = coleccion.PACKAGE_TYPE,
                Descripcion = coleccion.PACKAGE_TYPE_NAME
            };
            return nuevoTipoColeccion;
        }
    }
}
