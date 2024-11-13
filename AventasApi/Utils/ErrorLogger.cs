using DBData.Database;
using System;
using System.Threading.Tasks;

namespace AventasApi.Utils
{
    public static class ErrorLogger
    {
        // Método asincrónico para registrar un error
        public static async Task LogErrorAsync(string errorCode, string controlador, string ruta, string usuario, string mensaje)
        {
            using (var ctx = new AVentasEntities())
            {
                var error = new ErroresLog()
                {
                    ErrorCode = errorCode,
                    Controlador = controlador,
                    Ruta = ruta,
                    Usuario = usuario,
                    fecha = DateTime.Now,
                    mensaje = mensaje
                };

                ctx.ErroresLog.Add(error);
                await ctx.SaveChangesAsync();
            }
        }
    }

}