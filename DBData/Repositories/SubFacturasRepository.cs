using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class SubFacturasRepository
    {
        public async Task<List<SubFacturasxCliente>> ObtenerSubFacturas()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.SubFacturasxCliente.AsNoTracking().ToList();
            }
        }
        public async Task GuardarSubFacturas(List<SubFacturasxCliente> subFacturas)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.SubFacturasxCliente.AddRange(subFacturas);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<SubFacturasxCliente>> ModificarOAgregarSubFacturas(List<SubFacturasxCliente> subFacturasAGuardar)
        {
            
            using (AVentasEntities context = new AVentasEntities())
            {
                var subFacturasBDD = context.SubFacturasxCliente;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var subFacturaAGuardar in subFacturasAGuardar)
                {
                    if (subFacturaAGuardar.Referencia == null)
                    {
                        return new List<SubFacturasxCliente>();
                    }
                    var subFacturaEnBD = subFacturasBDD.FirstOrDefault(subFac => subFac.Referencia == subFacturaAGuardar.Referencia);
                    if (subFacturaEnBD == null)
                    {
                        context.SubFacturasxCliente.Add(subFacturaAGuardar);
                    }
                    else
                    {
                        //subFacturaEnBD = new SubFacturasxCliente();

                        subFacturaEnBD.Factura = subFacturaAGuardar.Factura;
                        subFacturaEnBD.CodigoCliente = subFacturaAGuardar.CodigoCliente;
                        subFacturaEnBD.EmpresaId = subFacturaAGuardar.EmpresaId;
                        subFacturaEnBD.IdMoneda = subFacturaAGuardar.IdMoneda;
                        subFacturaEnBD.IdAcuerdoxCliente = subFacturaAGuardar.IdAcuerdoxCliente;
                        subFacturaEnBD.FechaVencimiento = subFacturaAGuardar.FechaVencimiento;
                        subFacturaEnBD.FechaMaxDescuento = subFacturaAGuardar.FechaMaxDescuento;
                        subFacturaEnBD.FechaVencimientoDescuento = subFacturaAGuardar.FechaVencimientoDescuento;
                        subFacturaEnBD.Saldo = subFacturaAGuardar.Saldo;
                        subFacturaEnBD.SaldoDivisa = subFacturaAGuardar.SaldoDivisa;
                        subFacturaEnBD.Descuento = subFacturaAGuardar.Descuento;
                        subFacturaEnBD.PendientePago = subFacturaAGuardar.PendientePago;
                        subFacturaEnBD.Referencia = subFacturaAGuardar.Referencia;
                        subFacturaEnBD.ReferenciaFacturas = subFacturaAGuardar.ReferenciaFacturas;
                        subFacturaEnBD.ReferenciaAcuerdo = subFacturaAGuardar.ReferenciaAcuerdo;
                        subFacturaEnBD.NumeroCuota = subFacturaAGuardar.NumeroCuota;
                        subFacturaEnBD.ValorCuota = subFacturaAGuardar.ValorCuota;
                        subFacturaEnBD.ValorVencidoCuota = subFacturaAGuardar.ValorVencidoCuota;
                        subFacturaEnBD.ReferenciaCuotas = subFacturaAGuardar.ReferenciaCuotas;
                        subFacturaEnBD.IdFactura = subFacturaAGuardar.IdFactura;
                    }
                }
                await context.SaveChangesAsync();
                return subFacturasBDD.AsNoTracking().ToList();
            }
        }
    }
}
