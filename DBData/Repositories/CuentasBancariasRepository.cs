using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using DBData.Utils;
using System.Threading.Tasks;
using System.Data.Entity;

namespace DBData.Repositories
{
    public class CuentasBancariasRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task SendToDatabase(List<CuentasBancarias> cuentasBancarias)
        {
            if (LogicValidation.ValidateDataCount(cuentasBancarias.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                foreach (var cuenta in cuentasBancarias)
                {
                    if (LogicValidation.IsDataValid(cuenta))
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            var cuentaBD = await context.CuentasBancarias.FirstOrDefaultAsync(x => x.NumeroCuenta == (cuenta.NumeroCuenta ?? " "));

                            var banco = await context.Bancos.FirstOrDefaultAsync(x => x.NombreBanco == (cuenta.GrupoBanco ?? " ") ||
                            x.Descripcion == (cuenta.GrupoBanco ?? " ") || x.Descripcion == (cuenta.Descripcion ?? " ")) ??
                            await context.Bancos.FirstOrDefaultAsync(x => x.NombreBanco.Contains(cuenta.GrupoBanco)) ??
                            await context.Bancos.FirstOrDefaultAsync(x => x.Descripcion == cuenta.Descripcion);

                            if (LogicValidation.IsDataValid(cuentaBD))
                            {
                                updateCount++;
                                context.Entry(cuentaBD).State = EntityState.Modified;
                                cuentaBD.NombreBanco = cuenta.NombreBanco;
                                cuentaBD.NumeroCuenta = cuenta.NumeroCuenta;
                                cuentaBD.Descripcion = cuenta.Descripcion;
                                cuentaBD.GrupoBanco = cuenta.GrupoBanco;
                                cuentaBD.IdBanco = banco?.IdBanco;
                                cuentaBD.IdMoneda = cuenta.IdMoneda;
                                cuentaBD.EmpresaId = cuenta.EmpresaId;

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
                                errorCount += await CreandoCuenta(cuenta, banco?.IdBanco);
                            }
                        }
                    }
                }
                string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                LogicValidation.EmailNotification("GestorCuentasBancarias", counter);
            }
        }

        public static string GrupoBanco(string grupoBanco)
        {
            var valor = " ";
            if (LogicValidation.IsDataValid(grupoBanco))
            {
                string[] banco = grupoBanco.Split('-');
                if (LogicValidation.ValidateDataCountWithRestriction(banco.Count(), 1))
                {
                    valor = banco[0] + " " + banco[1];
                }
            }
            return valor;
        }

        public async Task<int> CreandoCuenta(CuentasBancarias cuenta, int? idBanco)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                cuenta.IdBanco = idBanco;
                context.CuentasBancarias.Add(cuenta);

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
