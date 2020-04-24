using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class ColeccionesRepository
    {
        public async Task<List<Colecciones>> ObtenerColecciones()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.Colecciones.AsNoTracking().ToList();
            }
        }
        public async Task GuardarColecciones(List<Colecciones> coleccionesAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Colecciones.AddRange(coleccionesAGuardar);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<Colecciones>> ModificarOAgregarColecciones(List<Colecciones> coleccionesAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var coleccionesEnDB = context.Colecciones;
                //foreach (var coleccion in coleccionesEnDB)
                //{
                //    coleccion.Status = false;
                //}
                foreach (var coleccionAGuardar in coleccionesAGuardar)
                {
                    var coleccionEnBD = coleccionesEnDB.FirstOrDefault(col => col.CodigoColeccion == coleccionAGuardar.CodigoColeccion);
                    if (coleccionEnBD == null)
                    {
                        context.Colecciones.Add(coleccionAGuardar);
                    }
                    else
                    {

                        coleccionEnBD.IdColeccion = coleccionAGuardar.IdColeccion;
                        coleccionEnBD.CodigoColeccion = coleccionAGuardar.CodigoColeccion;
                        coleccionEnBD.Nombre = coleccionAGuardar.Nombre;
                        coleccionEnBD.ColeccionTipo = coleccionAGuardar.ColeccionTipo;
                        coleccionEnBD.EmpresaId = coleccionAGuardar.EmpresaId;
                        coleccionEnBD.DisenoInicio = coleccionAGuardar.DisenoInicio;
                        coleccionEnBD.DisenoFinal = coleccionAGuardar.DisenoFinal;
                        coleccionEnBD.EntregaInicio = coleccionAGuardar.EntregaInicio;
                        coleccionEnBD.EntregaFinal = coleccionAGuardar.EntregaFinal;
                        coleccionEnBD.Estatus = coleccionAGuardar.Estatus;
                        coleccionEnBD.ProduccionInicio = coleccionAGuardar.ProduccionInicio;
                        coleccionEnBD.ProduccionFinal = coleccionAGuardar.ProduccionFinal;
                        coleccionEnBD.VentaInicio = coleccionAGuardar.VentaInicio;
                        coleccionEnBD.VentaFinal = coleccionAGuardar.VentaFinal;
                        coleccionEnBD.FotoPortada = coleccionAGuardar.FotoPortada;
                    }
                }
                await context.SaveChangesAsync();
                return coleccionesEnDB.AsNoTracking().ToList();
            }
        }
    }
}
