using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class ColoresXProductoRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task SendToDatabase(List<ColoresxProducto> colores, string CodigoColeccion)
        {
            if (logicValidation.ValidateDataCount(colores.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                using (AVentasEntities context = new AVentasEntities())
                {
                    foreach (var color in colores)
                    {
                        if (logicValidation.IsDataValid(color))
                        {
                            var productoXColeccion = await context.ProductosxColeccion.FirstOrDefaultAsync(prod => prod.CodigoProducto == CodigoColeccion);
                            if (logicValidation.IsDataValid(productoXColeccion))
                            {
                                var codigoColor = logicValidation.SeparatorProperty(color.CodigoColor, 1);
                                var colorBD = context.ColoresxProducto.FirstOrDefault(col => col.IdProducto == productoXColeccion.IdProducto
                                               && col.CodigoColor == codigoColor);
                                if (!logicValidation.IsDataValid(colorBD))
                                {
                                    insertCount++;
                                    errorCount += await CreandoColor(color, productoXColeccion.IdProducto);
                                }
                            }
                        }
                    }
                    string collection =  "Cod " + CodigoColeccion;
                    string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                    logicValidation.EmailNotificationWithCollection("GestorColoresXProducto", counter, collection);
                }
            }
        }

        public async Task<int> CreandoColor(ColoresxProducto color, int? IdProducto)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                color.IdProducto = IdProducto;
                color.CodigoColor = logicValidation.SeparatorProperty(color.CodigoColor, 1);
                context.ColoresxProducto.Add(color);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception) { contador++; }
            }
            return contador;
        }
    }
}
