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
    public class FacturasManager
    {
        public async Task<List<FacturasXClienteApiModel>> ObtenerFacturas(string empresa, string usuarioAsesor, string ClienteId)
        {
            GestorFacturasXCliente facturaManager = new GestorFacturasXCliente();
            var facturas = facturaManager.ObtenerFacturas(empresa, usuarioAsesor, ClienteId).Result;
            if (facturas != null && facturas.Count > 0)
            {
                return facturas;
            }
            return facturas;
        }
        public async Task<List<FacturasxCliente>> GuardarFacturas(List<FacturasXClienteApiModel> facturasAguradar)
        {
            TipoPedidoRepository acuRepository = new TipoPedidoRepository();
            var tiposPedido = acuRepository.ObtenerTiposdePedido().Result;
            FacturasRepository facturasRepository = new FacturasRepository();
            var subFacturasBD = facturasAguradar.Select(subFac => subFac.ToFacturasxCliente(tiposPedido)).ToList();
            return facturasRepository.ModificarOAgregarFacturas(subFacturasBD).Result;
        }
        public async Task IniciarProceso()
        {
            ClientesRepository clienteRepository = new ClientesRepository();
            AsesoresRepository asesorRepository = new AsesoresRepository();
            var asesores = asesorRepository.ObtenerAsesores().Result;
            foreach (var asesor in asesores)
            {
                var clientes = clienteRepository.ObtenerClientes(asesor.CodigoAsesor).Result.ToList();
                foreach (var cliente in clientes)
                {
                    var facturas = ObtenerFacturas(asesor.EmpresaId, asesor.Usuario, cliente.CodigoCliente).Result;
                    GuardarFacturas(facturas).Wait();
                }
            }

        }
    }
}
