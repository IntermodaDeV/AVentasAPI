using DBData.Database;
using ExternalApiData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class ProductosxColeccionExtension
    {
        public static ProductosxColeccion ToProductosxColeccion(this ProductoXColeccionApiModel prod, List<MaestroLinea> Lineas, int IdColeccion)
        {
            var linea = Lineas.FirstOrDefault(ml => ml.Linea == prod.linea);
            if (linea != null && prod.grupoTallaId != "")
            {
                ProductosxColeccion producto = new ProductosxColeccion
                {
                    CodigoProducto = prod.productoId,
                    IdColeccion = IdColeccion,
                    NombreProducto = prod.nombreProducto,
                    BackOrder = prod.backorder == 0 ? false : true,
                    Multiplo = prod.multiplo,
                    IdLinea = linea.IdLinea,
                    CodigoGrupoTalla = prod.grupoTallaId,

                };
                producto.IdEdad = prod.edad[0]?.codigo;
                return producto;
            }
            return null;

        }
    }
}
