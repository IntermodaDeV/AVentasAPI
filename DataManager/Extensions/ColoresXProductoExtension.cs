using DBData.Database;
using ExternalApiData.Models.ApiModels;

namespace DataManager.Extensions
{
    public static class ColoresXProductoExtension
    {
        public static ColoresxProducto CreandoColor(this ColorXProductoCRMApiModel color)
        {
            ColoresxProducto nuevoColor = new ColoresxProducto()
            {
                CodigoColor = color.COLORCODE + "-" + (color.PRODUCT ?? " "),
                IdProducto = null,
                Disponible = null
            };
            return nuevoColor;
        }
    }
}
