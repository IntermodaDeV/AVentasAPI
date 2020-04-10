using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class RutasXAsesorRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        //Revisar Logica
        public async Task SendToDatabase(List<RutasxAsesor> rutas, string EmpresaId, string Diario, string CodigoAsesor)
        {
            if (logicValidation.ValidateDataCount(rutas.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                using (AVentasEntities context = new AVentasEntities())
                {
                    foreach (var ruta in rutas)
                    {
                        if (logicValidation.IsDataValid(ruta))
                        {
                            var code = logicValidation.SeparatorProperty(ruta.CodigoRuta, 2);
                            if (logicValidation.ValidateDataCountWithRestriction(code.Length, 1))
                            {
                                insertCount++;
                                errorCount += await CreandoAtributo(ruta, CodigoAsesor ?? " ");
                            }
                        }
                    }
                    string coleccion = "Asesor: " + CodigoAsesor;
                    string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                    logicValidation.EmailNotificationWithCollection("GestorRutasXAsesor", counter, coleccion);
                }
            }
        }

        public async Task<int> CreandoAtributo(RutasxAsesor ruta, string CodigoAsesor)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                ruta.CodigoAsesor = CodigoAsesor;
                context.RutasxAsesor.Add(ruta);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception) { contador++; }
            }
            return contador;
        }
    }
}
