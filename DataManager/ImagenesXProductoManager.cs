using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using ExternalApiData.Models.ApiModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class ImagenesXProductoManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerImagenes(string CodigoColeccion)
        {
            GestorImagenesXProducto gestorImagenes = new GestorImagenesXProducto();

            var imagenesXproducto = gestorImagenes.ObtenerImagenesDesdeCRMAPI(CodigoColeccion ?? " ").Result;
            if (LogicValidation.ValidateDataCount(imagenesXproducto.Count))
            {
                var imagenes = imagenesXproducto.Select(atr => atr.CreandoImagen()).ToList();
                ImagenesXProductoRepository imagenesRepository = new ImagenesXProductoRepository();
                await imagenesRepository.SendToDatabase(imagenes, CodigoColeccion);
            }
        }
    }
}
