using DBData.Database;
using DBData.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataManager.Extensions;
using Proxy;

namespace DataManager
{
    public class GrupoImpuestoClientesManager 
    {
        public async Task<List<GrupoImpuestoCliente>> GuardarGrupoImpClientes(List<IMObtenerGrupoImpuestoClientes_Result> GrupoCliente)
        {
            GrupoImpuestoClientesRepository ImpClienteRepository = new GrupoImpuestoClientesRepository();
            return ImpClienteRepository.ModificarOAgregarGrupoImpuestoClientes(GrupoCliente.Select(cli => cli.ToImpuestoClientes()).ToList()).Result;
        }
        public async Task IniciarProceso()
        {
            EmpresaRepository EmpresaRepository = new EmpresaRepository();
            var Empresas = EmpresaRepository.ObtenerEmpresa().Result;
            foreach (var Empresa in Empresas)
            {
                var GrupoImpClientes = Proxy.Proxy.GetGrupoImpuestoClientes(Empresa.EmpresaId);
                GuardarGrupoImpClientes(GrupoImpClientes).Wait();
            }

        }
    }
}
