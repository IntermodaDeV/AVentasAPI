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
    public class SubfacturasManager
    {
        public async Task<List<SubFacturasXClienteApiModel>> ObtenerSubFacturas(string empresa, string Asesor, string ClienteId)
        {
            GestorSubFacturasXCliente subFacturaManager = new GestorSubFacturasXCliente();
            var subfacturas = subFacturaManager.ObtenerSubFacturas(empresa, Asesor, ClienteId).Result;
            if (subfacturas != null && subfacturas.Count > 0)
            {
                return subfacturas;
            }
            return subfacturas;
        }
        public async Task<List<SubFacturasxCliente>> GuardarSubFacturas(List<SubFacturasXClienteApiModel> subFacturasAguradar)
        {
            AcuerdosxClienteRepository acuRepository = new AcuerdosxClienteRepository();
            var acuerdos = acuRepository.ObtnerAcuerdos().Result;
            FacturasRepository facRepo = new FacturasRepository();
            var facturas = facRepo.ObtenerFacturas().Result;
            SubFacturasRepository subFacturasRepository = new SubFacturasRepository();
            var subFacturasBD = subFacturasAguradar.Select(subFac => subFac.ToSubFacturasxCliente(facturas, acuerdos)).ToList();
        
            return subFacturasRepository.ModificarOAgregarSubFacturas(subFacturasBD).Result;
            
        }
        public async Task IniciarProceso()
        {
            ClientesRepository clienteRepository = new ClientesRepository();
            AsesoresRepository asesorRepository = new AsesoresRepository();
            var asesores = asesorRepository.ObtenerAsesores().Result;
            foreach (var asesor in asesores)
            {
                var clientesIds = clienteRepository.ObtenerClientes(asesor.CodigoAsesor).Result.ToList();
                foreach (var clienteId in clientesIds)
                {
                    var colecciones = ObtenerSubFacturas(asesor.EmpresaId, asesor.Usuario, clienteId.CodigoCliente).Result;
                    GuardarSubFacturas(colecciones).Wait();
                }
            }

        }
    }
}
