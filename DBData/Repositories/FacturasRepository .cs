using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class FacturasRepository
    {
        public async Task<List<FacturasxCliente>> ObtenerFacturas()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.FacturasxCliente.AsNoTracking().ToList();
            }
        }
        public async Task GuardarFacturas(List<FacturasxCliente> facturas)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.FacturasxCliente.AddRange(facturas);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<FacturasxCliente>> ModificarOAgregarFacturas(List<FacturasxCliente> facturasAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var facturasBDD = context.FacturasxCliente;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var facturaAGuardar
                    in facturasAGuardar)
                {
                    var facturaEnBD = facturasBDD.FirstOrDefault(subFac => subFac.Referencia == facturaAGuardar.Referencia);
                    if (facturaEnBD == null)
                    {
                        context.FacturasxCliente.Add(facturaAGuardar);
                    }
                    else
                    {
                        facturaEnBD = new FacturasxCliente();

                        facturaEnBD.IdFactura = facturaAGuardar.IdFactura;
                        facturaEnBD.Factura = facturaAGuardar.Factura;
                        facturaEnBD.CodigoCliente = facturaAGuardar.CodigoCliente;
                        facturaEnBD.EmpresaId = facturaAGuardar.EmpresaId;
                        facturaEnBD.IdMoneda = facturaAGuardar.IdMoneda;
                        facturaEnBD.Tipo = facturaAGuardar.Tipo;
                        facturaEnBD.FechaFactura = facturaAGuardar.FechaFactura;
                        facturaEnBD.FechaVencimiento = facturaAGuardar.FechaVencimiento;
                        facturaEnBD.FechaMaxDescuento = facturaAGuardar.FechaMaxDescuento;
                        facturaEnBD.TotalFactura = facturaAGuardar.TotalFactura;
                        facturaEnBD.Saldo = facturaAGuardar.Saldo;
                        facturaEnBD.PendienteFactura = facturaAGuardar.PendienteFactura;
                        facturaEnBD.Descuento = facturaAGuardar.Descuento;
                        facturaEnBD.FacturaStatus = facturaAGuardar.FacturaStatus;
                        facturaEnBD.NumeroPagos = facturaAGuardar.NumeroPagos;
                        facturaEnBD.Referencia = facturaAGuardar.Referencia;
                        facturaEnBD.IdLinea = facturaAGuardar.IdLinea;
                        facturaEnBD.IdTipoPedido = facturaAGuardar.IdTipoPedido;
                        facturaEnBD.IdAcuerdoxCliente = facturaAGuardar.IdAcuerdoxCliente;
                        facturaEnBD.NumeroFEL = facturaAGuardar.NumeroFEL;
                    }
                }
                await context.SaveChangesAsync();
                return facturasBDD.AsNoTracking().ToList();
            }
        }
    }
}
