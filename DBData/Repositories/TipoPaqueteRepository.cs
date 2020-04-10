using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class TipoPaqueteRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task SendToDatabase(List<TiposdeColeccion> tipos)
        {
            int updateCount = 0, insertCount = 0, errorCount = 0;
            foreach (var tipo in tipos)
            {
                if (logicValidation.IsDataValid(tipo))
                {
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        var tipoBD = await context.TiposdeColeccion.FirstOrDefaultAsync(tip => tip.ColeccionTipo ==
                                        tipo.ColeccionTipo && tip.Descripcion == tipo.Descripcion);
                        if (!logicValidation.IsDataValid(tipoBD))
                        {
                            insertCount++;
                            errorCount += await CreandoTipoPaquete(tipo);
                        }
                    }
                }
            }
            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
            logicValidation.EmailNotification("GestorTipoPaquete", counter);
        }

        public async Task<int> CreandoTipoPaquete(TiposdeColeccion tipo)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                context.TiposdeColeccion.Add(tipo);

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
