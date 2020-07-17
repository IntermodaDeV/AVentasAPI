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
    public class AsesoresRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task<List<Asesores>> ObtenerAsesores()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.Asesores.AsNoTracking().ToList();
            }
        }

        public async Task SendToDatabase(List<Asesores> ListaAsesores)
        {
            int updateCount = 0, insertCount = 0, errorCount = 0;
            foreach (var asesor in ListaAsesores)
            {
                if (logicValidation.IsDataValid(asesor))
                {
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        var asesorBD = await context.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor == asesor.CodigoAsesor
                                        && x.EmpresaId == asesor.EmpresaId);
                        if (logicValidation.IsDataValid(asesorBD))
                        {
                            var asesorModel = new AsesoresAPIViewModel(asesorBD);
                            var asesorAPI = new AsesoresAPIViewModel(asesor);

                            bool resul = logicValidation.ValidateModels(asesorModel, asesorAPI);
                            if (!resul)
                            {
                                updateCount++;
                                context.Entry(asesorBD).State = EntityState.Modified;
                                asesorBD.CodigoAsesor = asesor.CodigoAsesor;
                                asesorBD.Nombre = asesor.Nombre;
                                asesorBD.EmpresaId = asesor.EmpresaId;
                                asesorBD.Usuario = asesor.Usuario;
                                asesorBD.Diario = asesor.Diario;

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
                        }
                        else
                        {
                            insertCount++;
                            errorCount += await CreandoAsesor(asesor);
                        }
                    }
                }
            }
            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
            logicValidation.EmailNotification("GestorAsesores", counter);
        }

        public class AsesoresAPIViewModel
        {
            public string CodigoAsesor { get; set; }
            public string Nombre { get; set; }
            public string EmpresaId { get; set; }
            public string Usuario { get; set; }
            public string Diario { get; set; }

            public AsesoresAPIViewModel(Asesores asesor)
            {
                CodigoAsesor = asesor.CodigoAsesor;
                Nombre = asesor.Nombre;
                EmpresaId = asesor.EmpresaId;
                Usuario = asesor.Usuario;
                Diario = asesor.Diario;
            }
        }

        public async Task<int> CreandoAsesor(Asesores asesor)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Asesores.Add(asesor);

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
