using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Linq;

namespace DataManager.Extensions
{
    public static class ImagenesXProductoExtension
    {
        public static FotografiasXProducto CreandoImagen(this ImageneXProductoXColorApiModel imagen)
        {
            FotografiasXProducto nuevaImagen = new FotografiasXProducto
            {
                IdProducto = 1,
                Codigo = imagen.ITEM_CODE,
                CodigoColor = imagen.ITEM_COLOR,
                Descripcion = null,
                FotografiaProducto = UrlImagen(imagen.IMAGE_PATH),
                Principal =  (imagen.IMAGE_MAIN == "1") ? true : false,
                
            };
            return nuevaImagen;
        }

        public static string UrlImagen(string imagen)
        {
            string nombreImagen = " ";
            try
            {
                nombreImagen = (imagen != null) ? imagen.Split('\\').Last() : " ";
            }
            catch (Exception) { }
            return nombreImagen;
        }
    }
}
