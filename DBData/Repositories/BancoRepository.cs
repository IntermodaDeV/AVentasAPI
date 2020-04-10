using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBData.Utils;
using System.Data.Entity;

namespace DBData.Repositories
{
    public class BancosRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task SendToDatabase(List<Bancos> listaBancos)
        {
            int updateCount = 0, insertCount = 0, errorCount = 0;
            foreach (var banco in listaBancos)
            {
                if (LogicValidation.IsDataValid(banco))
                {
                    using (AVentasEntities context = new AVentasEntities())
                    {
                        var bancoBD = await context.Bancos.FirstOrDefaultAsync(x => x.NombreBanco == banco.NombreBanco);
                        if (LogicValidation.IsDataValid(bancoBD))
                        {
                            updateCount++;
                            context.Entry(bancoBD).State = EntityState.Modified;
                            bancoBD.NombreBanco = banco.NombreBanco;
                            bancoBD.Descripcion = banco.Descripcion;
                            bancoBD.EmpresaId = banco.EmpresaId;

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
                            errorCount += await CreandoBanco(banco);
                        }
                    }
                }
            }
            string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
            LogicValidation.EmailNotification("GestorBancos", counter);
        }

        public async Task<int> CreandoBanco(Bancos banco)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Bancos.Add(banco);

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
