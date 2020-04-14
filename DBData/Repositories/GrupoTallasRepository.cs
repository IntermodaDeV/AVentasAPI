using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class GrupoTallasRepository
    {
        public async Task<List<GrupoTalla>> ObtenerGruposTallas()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.GrupoTalla.AsNoTracking().ToList();

            }

        }
        public async Task<List<GrupoTalla>> ModificarOAgregar(List<GrupoTalla> gruposTallaAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var gruposTallasEnBD = context.GrupoTalla;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var grupoTallaAGuardar in gruposTallaAGuardar)
                {
                    var lineaEnBD = gruposTallasEnBD.FirstOrDefault(col => col.CodigoGrupoTalla == grupoTallaAGuardar.CodigoGrupoTalla);
                    if (lineaEnBD == null)
                    {
                        context.GrupoTalla.Add(grupoTallaAGuardar);
                    }
                    else
                    {

                        lineaEnBD.CodigoGrupoTalla = grupoTallaAGuardar.CodigoGrupoTalla;
                        lineaEnBD.Descripcion = grupoTallaAGuardar.Descripcion;
                    }
                }
                await context.SaveChangesAsync();
                return gruposTallasEnBD.AsNoTracking().ToList();
            }
        }
    }
}
