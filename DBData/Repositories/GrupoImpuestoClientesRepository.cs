using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class GrupoImpuestoClientesRepository
    {
        public async Task<List<GrupoImpuestoCliente>> ObtenerGrupoImpuestoArticulos()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.GrupoImpuestoCliente.AsNoTracking().ToList();
            }
        }
        
        public async Task GuardarGrupoImpuestoClientes(List<GrupoImpuestoCliente> GrupoImpuestoCliente)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.GrupoImpuestoCliente.AddRange(GrupoImpuestoCliente);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<GrupoImpuestoCliente>> ModificarOAgregarGrupoImpuestoClientes(List<GrupoImpuestoCliente> GrupoImpuestoClientes)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var GrupoImpuestoClienteEnDB = context.GrupoImpuestoCliente;
                foreach (var ImpuestoClientes in GrupoImpuestoClientes)
                {
                    var ImpuestoClientesEnDB = GrupoImpuestoClienteEnDB.FirstOrDefault(col => col.GrupoCliente == ImpuestoClientes.GrupoCliente && col.GrupoImpuesto == ImpuestoClientes.GrupoImpuesto && col.Porcentaje == ImpuestoClientes.Porcentaje && col.Empresa == ImpuestoClientes.Empresa);
                    if (ImpuestoClientesEnDB == null)
                    {
                        context.GrupoImpuestoCliente.Add(ImpuestoClientes);
                    }
                    else
                    {
                        ImpuestoClientesEnDB = new GrupoImpuestoCliente();

                        ImpuestoClientesEnDB.GrupoCliente = ImpuestoClientes.GrupoCliente;
                        ImpuestoClientesEnDB.GrupoImpuesto = ImpuestoClientes.GrupoImpuesto;
                        ImpuestoClientesEnDB.Porcentaje = ImpuestoClientes.Porcentaje;
                        ImpuestoClientesEnDB.Empresa = ImpuestoClientes.Empresa;
                    }
                }
                await context.SaveChangesAsync();
                return GrupoImpuestoClienteEnDB.AsNoTracking().ToList();
            }
        }
    }
}
