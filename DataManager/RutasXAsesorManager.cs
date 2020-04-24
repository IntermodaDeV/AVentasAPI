using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class RutasXAsesorManager
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task ObtenerRutas(string EmpresaId, string Diario, string CodigoAsesor)
        {
            GestorRutasXAsesor gestorRutas = new GestorRutasXAsesor();

            var rutasAsesores = gestorRutas.ObtenerRutasDesdeCRMAPI(EmpresaId ?? " ", Diario ?? " ").Result;
            if (logicValidation.ValidateDataCount(rutasAsesores.Count))
            {
                var rutas = rutasAsesores.Select(atr => atr.CreandoRutaXAsesor()).ToList();
                RutasXAsesorRepository rutasRepository = new RutasXAsesorRepository();
                await rutasRepository.SendToDatabase(rutas, EmpresaId, Diario, CodigoAsesor);
            }
        }
    }
}
