using DBData.Database;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager.Extensions;
using ExternalApiData.Models.ApiModels;
using Proxy;

namespace DataManager
{
    public class GrupoImpuestoArticulosManager 
    {
        public async Task<List<GrupoImpuestoArticulo>> GuardarGrupoArticulo(List<IMObtenerGrupoImpuestoArticulos_Result> GrupoArticulo)
        {
            GrupoImpuestoArticulosRepository ArticuloRepository = new GrupoImpuestoArticulosRepository();
            return ArticuloRepository.ModificarOAgregarGrupoImpuestoArticulos(GrupoArticulo.Select(cli => cli.ToImpuestoArticulos()).ToList()).Result;
        }
        public async Task IniciarProceso()
        {
            EmpresaRepository EmpresaRepository = new EmpresaRepository();
            var Empresas = EmpresaRepository.ObtenerEmpresa().Result;
            foreach (var Empresa in Empresas)
            {
                var GrupoArticulo = Proxy.Proxy.GetGrupoImpuestoArticulos(Empresa.EmpresaId);
                GuardarGrupoArticulo(GrupoArticulo).Wait();
            }

        }
    }
}
