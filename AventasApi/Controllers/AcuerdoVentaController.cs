using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class AcuerdoVentaController : ApiController
    {
        [HttpGet]
        [Route("~/api/acuerdo/cuotas/{codigoCliente}")]
        public async Task<IHttpActionResult> ObtenerCuotasDeAcuerdo(string codigoCliente)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var AcuerdosVenta = await ctx.AcuerdosxCliente.Where(x=> x.Desde <= DateTime.Now && x.Hasta >= DateTime.Now && x.CodigoCliente == codigoCliente).Select(x => new AcuerdoVentaViewModel { 
                        IdAcuerdoxCliente = x.IdAcuerdoxCliente,
                        CodigoCliente = x.CodigoCliente,
                        IdMoneda = x.IdMoneda,
                        Total = x.Total,
                        Saldo = x.Saldo,
                        Liberado = x.Liberado,
                        Facturado = x.Facturado,
                        Entregado = x.Entregado,
                        Linea = x.IdLinea,
                        Desde = x.Desde,
                        Hasta = x.Hasta,
                        CodigoDescuento = x.GrupoDescuento,
                        EmpresaId = x.EmpresaId,
                        CuotasDeAcuerdo = ctx.CuotasXAcuerdo.Where(a => a.IdAcuerdoVenta == x.IdAcuerdoxCliente).Select(a => new CuotasDeAcuerdoViewModel { 
                            NumCuota = a.NumCuota,
                            ValorCuota = a.ValorCuota,
                            SaldoDisponible = a.SaldoDiponible,
                            FechaVencimiento = a.FechaVencimiento,
                            Descuento = 0,
                            FacturasCuotas = ctx.FacturasEnCuotasAcuerdos.Where(f => f.idCuotaXAcuerdo == a.IdCuotasXAcuerdoVenta).Select(f => new FacturasEnCuotasAcuerdoViewModel { 
                                IdCuotaXAcuerdo = f.idCuotaXAcuerdo,
                                Factura = f.Factura,
                                Valor = f.Valor,
                                FechaFactura = f.FechaFactura,
                                FechaVencimiento = f.FechaVencimiento,
                                PagosEnFacturas = ctx.PagosAFacturasDeCuotas.Where(p => p.IdFacturaXCuota == f.IdFacturaXCuota).Select(p => new PagosAFacturasXCuotaViewModel
                                {
                                    IdFacturaXCuota = p.IdFacturaXCuota,
                                    NumeroDocumento = p.NumeroDocumento,
                                    Valor = p.Valor,
                                    FechaLiquidacion = p.FechaLiquidacion,
                                    FechaDeposito = p.FechaDeposito
                                }).ToList()
                            }).ToList(),
                        }).OrderBy(a => a.NumCuota).ToList(),
                    }).ToListAsync();

                    foreach (var acuerdo in AcuerdosVenta)
                    {
                        var GrupoDescuentoAcuerdo = ctx.DescuentoEnAcuerdo.FirstOrDefault(x => x.CodigoDescuento == acuerdo.CodigoDescuento && x.empresaId == acuerdo.EmpresaId);
                        if (GrupoDescuentoAcuerdo == null)
                        {
                            continue;
                        }

                        var asesor = ctx.Clientes.FirstOrDefault(x => x.CodigoCliente == codigoCliente);
                        if (asesor == null)
                        {
                            continue;
                        }

                        foreach (var cuota in acuerdo.CuotasDeAcuerdo)
                        {
                            var documentosAplicados = ctx.SP_DocumentosAplicadosXCuotas(asesor.CodigoAsesor).FirstOrDefault(x => x.NumeroCuota == cuota.NumCuota && x.CodigoCliente == acuerdo.CodigoCliente && x.IdAcuerdoxCliente == acuerdo.IdAcuerdoxCliente);
                            var FletePorCuota = documentosAplicados == null ? 0 : documentosAplicados.Flete;
                            var NotasAplicadasCuota = documentosAplicados == null ? 0 : documentosAplicados.Valor;
                            decimal consumidoCuota = cuota.ValorCuota - (cuota.SaldoDisponible ?? 0);
                            var valoCuota = consumidoCuota - FletePorCuota - NotasAplicadasCuota;
                            var DescuentoCuota = Math.Round(Convert.ToDouble(valoCuota * (GrupoDescuentoAcuerdo.Porcentaje / 100)), 2, MidpointRounding.AwayFromZero);
                            cuota.Descuento = DescuentoCuota;
                        }
                    }

                    return Ok(AcuerdosVenta);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
