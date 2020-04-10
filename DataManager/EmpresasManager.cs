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
    public class EmpresasManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerEmpresas()
        {
            GestorEmpresas gestorEmpresa = new GestorEmpresas();

            var empresas = gestorEmpresa.ObtenerEmpresasDesdeCRMAPI().Result;
            if (LogicValidation.ValidateDataCount(empresas.Count))
            {
                var listaEmpresas = empresas.Select(emp => emp.CreandoEmpresa()).ToList();
                EmpresaRepository empresaRepository = new EmpresaRepository();
                await empresaRepository.SendToDatabase(listaEmpresas);
            }
        }
    }
}
