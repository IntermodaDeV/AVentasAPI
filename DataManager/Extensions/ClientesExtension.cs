using DBData.Database;
using ExternalApiData.Models;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{

    public static class ClientesExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static Clientes ToClientes(this ClientesCRMApiModel cliente)
        {

            Clientes clienteAGuardar = new Clientes
            {
                CodigoCliente = cliente.ACCOUNT,
                EmpresaId = cliente.ENTITY,
                Nombre = cliente.NAME,
                ComunidadAutonoma = cliente.AUTONOMOUS_COMMUNITY,
                GrupoPrecio = cliente.PRICE,
                GrupoCliente = cliente.CUSTOMER_GROUP,
                Descuento = cliente.TOTAL_DISCOUNT,
                Direccion = cliente.ADDRESS,
                IdMoneda = cliente.CURRENCY,
                FacturacionEntrega = cliente.BLOCKED,
                GrupoImpuesto = cliente.TAX_GROUP,
                Telefono = cliente.PHONE
            };
            try
            {
                clienteAGuardar.CreditoDisponible = decimal.Parse(cliente.CREDIT_AVAILABLE);
            }
            catch (Exception) { }
            try
            {
                clienteAGuardar.LimiteCredito = decimal.Parse(cliente.CREDIT_LIMIT);
            }
            catch (Exception) { }

            return clienteAGuardar;
        }

    }

}
