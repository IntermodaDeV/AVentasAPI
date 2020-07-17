using DBData.Database;
using System.Collections.Generic;
using DBData.Utils;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using System.Linq;

namespace DBData.Repositories
{
    public class EmpresaRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<Empresa>> ObtenerEmpresa()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.Empresa.AsNoTracking().ToList();
            }
        }

        public async Task SendToDatabase(List<Empresa> empresas)
        {
            int updateCount = 0, insertCount = 0, errorCount = 0;
            foreach (var empresa in empresas)
            {
                if (LogicValidation.IsDataValid(empresa))
                {
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        var empresaBD = await context.Empresa.FindAsync(empresa.EmpresaId);
                        if (LogicValidation.IsDataValid(empresaBD))
                        {
                            updateCount++;
                            context.Entry(empresaBD).State = EntityState.Modified;
                            empresaBD.NombreEmpresa = empresa.NombreEmpresa;
                            empresaBD.Direccion = empresa.Direccion;
                            empresaBD.RegistroTributario = empresa.RegistroTributario;

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
                            errorCount += await CreandoEmpresa(empresa);
                        }
                    }
                }
            }
            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
            LogicValidation.EmailNotification("GestorEmpresas", counter);
        }

        public async Task<int> CreandoEmpresa(Empresa empresa)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Empresa.Add(empresa);

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
