using AventasApi.Utils;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Threading.Tasks;

namespace DataManager
{
    public class TipoPaqueteManager
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task ObtenerAtributos(string CodigoProducto)
        {
            GestorTipoPaquete gestorTipoPaquete = new GestorTipoPaquete();

            var listaTipos = gestorTipoPaquete.ObtenerTipoPaqueteDesdeCRMAPI().Result;
            if (logicValidation.ValidateDataCount(listaTipos.Count))
            {
                TipoPaqueteRepository tipoRepository = new TipoPaqueteRepository();
                await tipoRepository.SendToDatabase(listaTipos);
            }
        }
    }
}
