using DBData.Database;
using ExternalApiData.GestorData;
using System.Collections.Generic;
using System.Threading.Tasks;
using AventasApi.Utils;
using ExternalApiData.Models.ApiModels;
using System.Linq;
using DataManager.Extensions;
using DBData.Repositories;

namespace DataManager
{
    class EmpresasManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerEmpresas()
        {
            List<EmpresasCRMApiModel> empresas = new List<EmpresasCRMApiModel>();
            GestorEmpresas gestorEmpresa = new GestorEmpresas();

            empresas = gestorEmpresa.ObtenerEmpresas().Result;
            if (LogicValidation.ValidateDataCount(empresas.Count))
            {
                var empresasList = empresas.Select(acu => acu.CreandoEmpresa()).ToList();
                EmpresasRepository empresasRepository = new EmpresasRepository();
                await empresasRepository.SendToDatabase(empresasList);
            }
        }
    }
}
