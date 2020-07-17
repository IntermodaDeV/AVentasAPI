using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class ClientesRepository
    {
        public async Task<List<Clientes>> ObtenerClientes()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.Clientes.AsNoTracking().ToList();
            }
        }
        public async Task<List<Clientes>> ObtenerClientes(string codigoAsesor)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.RutasxAsesor.Where(rutAse => rutAse.CodigoAsesor == codigoAsesor).SelectMany(rutAse => rutAse.Rutas.ClientesxRuta).Select(cliRut => cliRut.Clientes).ToList();
            }
        }
        public async Task GuardarClientes(List<Clientes> clientesAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Clientes.AddRange(clientesAGuardar);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<Clientes>> ModificarOAgregarClientes(List<Clientes> clientesAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var clientesEnDB = context.Clientes;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var clienteAGuardar in clientesAGuardar)
                {
                    var clienteEnBD = clientesEnDB.FirstOrDefault(col => col.CodigoCliente == clienteAGuardar.CodigoCliente);
                    if (clienteEnBD == null)
                    {
                        context.Clientes.Add(clienteAGuardar);
                    }
                    else
                    {
                        clienteEnBD.CodigoCliente = clienteAGuardar.CodigoCliente;
                        clienteEnBD.EmpresaId = clienteAGuardar.EmpresaId;
                        clienteEnBD.Nombre = clienteAGuardar.Nombre;
                        clienteEnBD.Zona = clienteAGuardar.Zona;
                        clienteEnBD.ComunidadAutonoma = clienteAGuardar.ComunidadAutonoma;
                        clienteEnBD.GrupoPrecio = clienteAGuardar.GrupoPrecio;
                        clienteEnBD.GrupoCliente = clienteAGuardar.GrupoCliente;
                        clienteEnBD.Descuento = clienteAGuardar.Descuento;
                        clienteEnBD.Direccion = clienteAGuardar.Direccion;
                        clienteEnBD.IdMoneda = clienteAGuardar.IdMoneda;
                        clienteEnBD.FacturacionEntrega = clienteAGuardar.FacturacionEntrega;
                        clienteEnBD.Latitud = clienteAGuardar.Latitud;
                        clienteEnBD.Longitud = clienteAGuardar.Longitud;
                        clienteEnBD.Provincias = clienteAGuardar.Provincias;
                        clienteEnBD.Region = clienteAGuardar.Region;
                        clienteEnBD.Revision = clienteAGuardar.Revision;
                        clienteEnBD.LimiteCredito = clienteAGuardar.LimiteCredito;
                        clienteEnBD.CreditoDisponible = clienteAGuardar.CreditoDisponible;
                        clienteEnBD.Telefono = clienteAGuardar.Telefono;
                        clienteEnBD.GrupoImpuesto = clienteAGuardar.GrupoImpuesto;
                        clienteEnBD.ModoEntrega = clienteAGuardar.ModoEntrega;
                        context.SaveChanges();
                    }
                }
                await context.SaveChangesAsync();
                return clientesEnDB.AsNoTracking().ToList();
            }
        }
    }
}
