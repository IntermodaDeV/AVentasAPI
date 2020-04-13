using AventasApi.Utils;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Threading.Tasks;

namespace DataManager
{
    public class SizesByProductManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerTallasXProducto(string colleccionId)
        {
            GestorSizesByProduct gestorTipoPago = new GestorSizesByProduct();

            var tallas = gestorTipoPago.ObtenerTallasDesdeCRMAPI(colleccionId).Result;
            if (LogicValidation.ValidateDataCount(tallas.Count))
            {
                SizesByProductRepository tallaXproductoRepository = new SizesByProductRepository();
                await tallaXproductoRepository.SendToDatabase(tallas);
            }
        }
    }
}
