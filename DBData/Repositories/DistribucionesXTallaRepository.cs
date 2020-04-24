using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class DistribucionesXTallaRepository
    {
        public async Task<List<DistribucionxTalla>> Obtener()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.DistribucionxTalla.AsNoTracking().ToList();

            }

        }
        public async Task<List<DistribucionxTalla>> ModificarOAgregar(List<DistribucionxTalla> gruposTallaAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var gruposTallasEnBD = context.DistribucionxTalla;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var grupoTallaAGuardar in gruposTallaAGuardar)
                {
                    var lineaEnBD = gruposTallasEnBD.FirstOrDefault();
                    if (lineaEnBD == null)
                    {
                        context.DistribucionxTalla.Add(grupoTallaAGuardar);
                    }
                    else
                    {
                        lineaEnBD = new DistribucionxTalla();

                        //lineaEnBD.CodigoGrupoTalla = grupoTallaAGuardar.CodigoGrupoTalla;
                        //lineaEnBD.Descripcion = grupoTallaAGuardar.Descripcion;
                    }
                }
                await context.SaveChangesAsync();
                return gruposTallasEnBD.AsNoTracking().ToList();
            }
        }
    }
}
