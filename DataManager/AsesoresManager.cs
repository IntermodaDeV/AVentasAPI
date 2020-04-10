using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class AsesoresManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerEmpresas()
        {
            GestorAsesores gestorAsesores = new GestorAsesores();

            var asesores = gestorAsesores.ObtenerAsesoresDesdeCRMAPI().Result;
            if (LogicValidation.ValidateDataCount(asesores.Count))
            {
                var listaAsesores = asesores.Select(ase => ase.CreandoAsesor()).ToList();
                AsesoresRepository asesorRepository = new AsesoresRepository();
                await asesorRepository.SendToDatabase(listaAsesores);
            }
        }
    }
}
