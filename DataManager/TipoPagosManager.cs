using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class TipoPagosManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerTiposPago()
        {
            GestorTipoPagos gestorTipoPago = new GestorTipoPagos();

            var pagos = gestorTipoPago.ObtenerTiposDesdeCRMAPI().Result;
            if (LogicValidation.ValidateDataCount(pagos.Count))
            {
                var listaTipos = pagos.Select(tip => tip.CreandoTipoPago()).ToList();
                TipoPagosRepository tipoPagoRepository = new TipoPagosRepository();
                await tipoPagoRepository.SendToDatabase(listaTipos);
            }
        }
    }
}
