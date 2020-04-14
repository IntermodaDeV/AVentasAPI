using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class ProductosXColeccionRepository
    {
        public async Task<List<ProductosxColeccion>> ModificarOAgregar(List<ProductosxColeccion> productosAGuardar, int idColeccion)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var coleccionesEnDB = context.ProductosxColeccion.Where(prod => prod.IdColeccion == idColeccion);
                foreach (var productoAGuardar in productosAGuardar)
                {

                    var coleccionEnBD = coleccionesEnDB.FirstOrDefault(col => col.IdColeccion == idColeccion && col.CodigoProducto == productoAGuardar.CodigoProducto);
                    if (coleccionEnBD == null)
                    {
                        context.ProductosxColeccion.Add(productoAGuardar);
                    }
                    else
                    {

                        coleccionEnBD.IdProducto = productoAGuardar.IdProducto;
                        coleccionEnBD.CodigoProducto = productoAGuardar.CodigoProducto;
                        coleccionEnBD.IdColeccion = productoAGuardar.IdColeccion;
                        coleccionEnBD.NombreProducto = productoAGuardar.NombreProducto;
                        coleccionEnBD.CodigoGrupoTalla = productoAGuardar.CodigoGrupoTalla;
                        coleccionEnBD.BackOrder = productoAGuardar.BackOrder;
                        coleccionEnBD.Multiplo = productoAGuardar.Multiplo;
                        coleccionEnBD.IdEdad = productoAGuardar.IdEdad;
                    }
                }
                await context.SaveChangesAsync();
                return productosAGuardar;
            }
        }
    }
}
