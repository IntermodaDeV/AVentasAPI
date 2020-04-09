using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class AcuerdosxClienteRepository
    {
        public async Task GuardarAcuerdos(List<AcuerdosxCliente> acuerdosAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.AcuerdosxCliente.AddRange(acuerdosAGuardar);
                await context.SaveChangesAsync();
            }
        }
        public async Task ModificarOAgregarAcuerdos(List<AcuerdosxCliente> acuerdosAGuardar, string clienteID)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                List<string> idsAcuerdosXCliente = acuerdosAGuardar.Select(acuAGua => acuAGua.IdAcuerdoxCliente).ToList();
                var acuerdos = context.AcuerdosxCliente.Where(acuXCli => idsAcuerdosXCliente.Contains(acuXCli.IdAcuerdoxCliente));
                foreach (var acuerdoAGuardar in acuerdosAGuardar)
                {
                    var acuerdo = acuerdos.FirstOrDefault(acu => acu.IdAcuerdoxCliente == acuerdoAGuardar.IdAcuerdoxCliente);
                    if (acuerdo == null)
                    {
                        context.AcuerdosxCliente.Add(acuerdoAGuardar);
                    }
                    else
                    {
                        acuerdo.IdAcuerdoxCliente = acuerdoAGuardar.IdAcuerdoxCliente;
                        acuerdo.CodigoCliente = acuerdoAGuardar.CodigoCliente;
                        acuerdo.IdTipoPedido = acuerdoAGuardar.IdTipoPedido;
                        acuerdo.IdMoneda = acuerdoAGuardar.IdMoneda;
                        acuerdo.EmpresaId = acuerdoAGuardar.EmpresaId;
                        acuerdo.Total = acuerdoAGuardar.Total;
                        acuerdo.Saldo = acuerdoAGuardar.Saldo;
                        acuerdo.Liberado = acuerdoAGuardar.Liberado;
                        acuerdo.Facturado = acuerdoAGuardar.Facturado;
                        acuerdo.Entregado = acuerdoAGuardar.Entregado;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
