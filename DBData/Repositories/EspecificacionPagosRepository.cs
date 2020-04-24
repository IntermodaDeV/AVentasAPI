using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class EspecificacionPagosRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task SendToDatabase(List<TiposdePagoDetalle> listaTipos)
        {
            if (logicValidation.ValidateDataCount(listaTipos.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                foreach (var tipo in listaTipos)
                {
                    if (logicValidation.IsDataValid(tipo))
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            var tipoPagoBD = await context.TiposdePago.FirstOrDefaultAsync(pago => pago.Codigo == tipo.Codigo);
                            var detallePagoBD = await context.TiposdePagoDetalle.FirstOrDefaultAsync(tip => tip.Codigo == tipo.Codigo
                                            && tip.EmpresaId == tipo.EmpresaId);
                            if (logicValidation.IsDataValid(detallePagoBD))
                            {
                                updateCount++;
                                context.Entry(detallePagoBD).State = EntityState.Modified;
                                detallePagoBD.Codigo = tipo.Codigo;
                                detallePagoBD.Descripcion = tipo.Descripcion;
                                detallePagoBD.CodigoDetalle = tipo.CodigoDetalle;
                                detallePagoBD.EmpresaId = tipo.EmpresaId;
                                detallePagoBD.IdTipoPago = tipoPagoBD?.IdTipoPago;

                                try
                                {
                                    await context.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    Console.WriteLine(ex);
                                }
                            }
                            else
                            {
                                insertCount++;
                                errorCount += await CreandoDetalleTipoPago(tipo, tipoPagoBD?.IdTipoPago);
                            }

                        }
                    }
                }
                string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                logicValidation.EmailNotification("GestorEspecificacionPagos", counter);
            }
        }

        public async Task<int> CreandoDetalleTipoPago(TiposdePagoDetalle tipo, int? tipoPagoId)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                tipo.IdTipoPago = tipoPagoId;
                context.TiposdePagoDetalle.Add(tipo);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    contador++;
                    Console.WriteLine(ex);
                }
            }
            return contador;
        }
    }
}
