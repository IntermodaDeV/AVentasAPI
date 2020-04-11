using DBData.Database;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager.Extensions;

namespace DataManager
{
    public class ProductosXColeccionManager
    {
        public async Task<List<ProductosxColeccion>> Obtener(string codigoColeccion, int idColeccion, List<MaestroLinea> Lineas)
        {
            GestorProductos gestor = new GestorProductos();
            var colecciones = gestor.Obtener(codigoColeccion).Result;
            if (colecciones != null && colecciones.Count > 0)
            {
                return colecciones.Where(col => col != null).Select(col => col.ToProductosxColeccion(Lineas, idColeccion)).Where(col => col != null).ToList();
            }
            return null;
        }
        public async Task<List<ProductosxColeccion>> GuardarColecciones(List<ProductosxColeccion> coleccoinesAGuardar, int idColeccion)
        {
            ProductosXColeccionRepository respository = new ProductosXColeccionRepository();
            return respository.ModificarOAgregar(coleccoinesAGuardar, idColeccion).Result;
        }
        public async Task IniciarProceso()
        {
            //ObtenerColecciones
            ColeccionesRepository srepository = new ColeccionesRepository();
            LineasRepository lineaRepository = new LineasRepository();
            var lineas = lineaRepository.ObtenerLineas().Result;
            var colecciones = srepository.ObtenerColecciones().Result;
            foreach (var coleccion in colecciones)
            {
                var prooductosXColeccion = Obtener(coleccion.CodigoColeccion, coleccion.IdColeccion, lineas).Result; ;
                var productosGuardados = GuardarColecciones(prooductosXColeccion, coleccion.IdColeccion).Result;
            }
        }
    }
}
