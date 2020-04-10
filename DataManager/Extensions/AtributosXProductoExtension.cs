using DBData.Database;
using ExternalApiData.Models.ApiModels;

namespace DataManager.Extensions
{
    public static class AtributosXProductoExtension
    {
        public static AtributosxProducto CreandoAtributo(this AtributosXProductoCRMApiModel atributo)
        {
            AtributosxProducto nuevoAtributo = new AtributosxProducto()
            {
                CodigoAtributo = atributo.CODIGO,
                IdProducto = null,
                Descripcion1 = atributo.DESCRIPTION,
                Descripcion2 = atributo.DESCRIPTION2,
            };
            return nuevoAtributo;
        }
    }
}
