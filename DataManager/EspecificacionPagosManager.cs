using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class EspecificacionPagosManager
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task ObtenerTiposPago()
        {
            GestorEspecificacionPagos gestorEspecificaionPago = new GestorEspecificacionPagos();

            var pagos = gestorEspecificaionPago.ObtenerPagosDesdeCRMAPI().Result;
            if (logicValidation.ValidateDataCount(pagos.Count))
            {
                var listaTipos = pagos.Select(tip => tip.CreandoPago()).ToList();
                EspecificacionPagosRepository PagoRepository = new EspecificacionPagosRepository();
                await PagoRepository.SendToDatabase(listaTipos);
            }
        }
    }
}
