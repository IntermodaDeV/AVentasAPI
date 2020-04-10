using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class TipoPaqueteManager
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task ObtenerAtributos(string CodigoProducto)
        {
            GestorTipoPaquete gestorTipoPaquete = new GestorTipoPaquete();

            var listaTipos = gestorTipoPaquete.ObtenerTiposDesdeCRMAPI().Result;
            if (logicValidation.ValidateDataCount(listaTipos.Count))
            {
                var tiposPaquete = listaTipos.Select(tipo => tipo.CreandoTipoColeccion()).ToList();
                TipoPaqueteRepository tipoRepository = new TipoPaqueteRepository();
                await tipoRepository.SendToDatabase(tiposPaquete);
            }
        }
    }
}
