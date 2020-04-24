using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class TallasxGrupoRepository
    {
        public async Task<List<TallasXGrupo>> ObtenerTallasXGrupo()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.TallasXGrupo.AsNoTracking().ToList();

            }

        }
        public async Task<List<TallasXGrupo>> ModificarOAgregar(List<TallasXGrupo> gruposTallaAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var gruposTallasEnBD = context.TallasXGrupo;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var grupoTallaAGuardar in gruposTallaAGuardar)
                {
                    var lineaEnBD = gruposTallasEnBD.FirstOrDefault(col => col.CodigoGrupoTalla == grupoTallaAGuardar.CodigoGrupoTalla && col.CodigoTalla == grupoTallaAGuardar.CodigoTalla);
                    if (lineaEnBD == null)
                    {
                        context.TallasXGrupo.Add(grupoTallaAGuardar);
                    }
                    else
                    {
                        lineaEnBD.Orden = grupoTallaAGuardar.Orden;
                    }
                }
                await context.SaveChangesAsync();
                return gruposTallasEnBD.AsNoTracking().ToList();
            }
        }
    }
}
