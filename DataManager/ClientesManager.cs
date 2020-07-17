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

namespace DataManager
{
    public class ClientesManager
    {

        public async Task<List<ClientesCRMApiModel>> ObtnerClientes(string empresaId, string usuarioAsesor)
        {
            GestorClientes gestorCliente = new GestorClientes();
            var clientes = gestorCliente.ObtenerClientesXAsesor(empresaId, usuarioAsesor).Result;
            if (clientes != null && clientes.Count > 0)
            {
                return clientes;
            }
            return new List<ClientesCRMApiModel>();
        }
        public async Task<List<Clientes>> GuardarClientes(List<ClientesCRMApiModel> clientesAGuardar)
        {
            ClientesRepository cliRepository = new ClientesRepository();
            return cliRepository.ModificarOAgregarClientes(clientesAGuardar.Select(cli => cli.ToClientes()).ToList()).Result;
        }
        public async Task IniciarProceso()
        {

            AsesoresRepository asesorRepository = new AsesoresRepository();
            var asesores = asesorRepository.ObtenerAsesores().Result;
            foreach (var asesor in asesores)
            {
                var clientes = ObtnerClientes(asesor.EmpresaId, asesor.CodigoAsesor).Result.ToList();
                GuardarClientes(clientes).Wait();
            }

        }
    }
}
