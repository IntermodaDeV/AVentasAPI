using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DBData.Database;

namespace AventasApi.Utils
{
    public static class ConfiguracionCorreoHelper
    {
        // Devuelve las direcciones de correo ya resueltas (usuario del sistema -> su Correo, o el correo manual)
        // para un ConfiguracionCorreo y un tipo ('DESTINO' o 'COPIA').
        public static async Task<List<string>> ObtenerCorreosAsync(AVentasEntities ctx, int configuracionCorreoId, string tipo)
        {
            var destinatarios = await ctx.ConfiguracionCorreoUsuario
                .Where(x => x.ConfiguracionCorreoId == configuracionCorreoId && x.Tipo == tipo && x.Activo == true)
                .Select(x => new { x.UsuarioId, x.CorreoManual, CorreoUsuario = x.Usuarios.Correo })
                .ToListAsync();

            return destinatarios
                .Select(x => x.UsuarioId != null ? x.CorreoUsuario : x.CorreoManual)
                .Where(correo => !string.IsNullOrWhiteSpace(correo))
                .Distinct()
                .ToList();
        }
    }
}
