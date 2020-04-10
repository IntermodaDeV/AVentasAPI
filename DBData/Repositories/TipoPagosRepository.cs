using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class TipoPagosRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task SendToDatabase(List<TiposdePago> listaTipos)
        {
            if (LogicValidation.ValidateDataCount(listaTipos.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                foreach (var tipo in listaTipos)
                {
                    if (LogicValidation.IsDataValid(tipo))
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            var tipoPago = await context.TiposdePago.FirstOrDefaultAsync(x => x.Codigo == tipo.Codigo);
                            if (LogicValidation.IsDataValid(tipoPago))
                            {
                                updateCount++;
                                context.Entry(tipoPago).State = EntityState.Modified;
                                tipoPago.Codigo = tipo.Codigo;
                                tipoPago.Descripcion = tipo.Descripcion;
                                tipoPago.Tipo = tipo.Tipo;
                                tipoPago.EmpresaId = tipo.EmpresaId;

                                try
                                {
                                    await  context.SaveChangesAsync();
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
                                errorCount += await CreandoTipoPago(tipo);
                            }
                        }
                    }
                }
                string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                LogicValidation.EmailNotification("GestorTipoPagos", counter);
            }
        }

        public async Task<int> CreandoTipoPago(TiposdePago tipo)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                context.TiposdePago.Add(tipo);

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
