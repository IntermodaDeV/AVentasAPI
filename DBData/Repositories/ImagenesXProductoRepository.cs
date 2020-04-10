using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class ImagenesXProductoRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task SendToDatabase(List<FotografiasXProducto> ListaImagenes, string CodigoColeccion)
        {
            if (LogicValidation.ValidateDataCount(ListaImagenes.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                using (AVentasEntities context = new AVentasEntities())
                {
                    foreach (var imagen in ListaImagenes)
                    {
                        if (LogicValidation.IsDataValid(imagen))
                        {
                            var coleccionBD = await context.Colecciones.FirstOrDefaultAsync(x => x.CodigoColeccion == CodigoColeccion);
                            if (LogicValidation.IsDataValid(coleccionBD))
                            {
                                var producto = context.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == imagen.Codigo
                                                && prod.IdColeccion == coleccionBD.IdColeccion);
                                if (LogicValidation.IsDataValid(producto))
                                {
                                    var imagenBD = context.FotografiasXProducto.FirstOrDefault(img => img.IdProducto == producto.IdProducto
                                                    && img.FotografiaProducto == imagen.FotografiaProducto);
                                    if (LogicValidation.IsDataValid(imagenBD))
                                    {
                                        var imagenDeBD = new ImageneXProductoDTO(imagenBD, producto.CodigoProducto);
                                        var imagenDTO = new ImageneXProductoDTO(imagen, producto.CodigoProducto);

                                        bool resul = EvaluarModelos(imagenDeBD, imagenDTO);
                                        if (!resul)
                                        {
                                            updateCount++;
                                            context.Entry(imagenBD).State = EntityState.Modified;
                                            imagenBD.IdProducto = producto.IdProducto;
                                            imagenBD.CodigoColor = imagen.CodigoColor;
                                            imagenBD.FotografiaProducto = imagen.FotografiaProducto;
                                            imagenBD.Principal = imagen.Principal;

                                            try
                                            {
                                                await context.SaveChangesAsync();
                                            }
                                            catch (Exception ex)
                                            {
                                                errorCount++;
                                                Console.WriteLine(ex);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        insertCount++;
                                        errorCount += await CreandoFotografia(imagen, producto.IdProducto);
                                    }
                                }
                            }
                        }
                    }
                    string collection = "Cod " + CodigoColeccion;
                    string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                    LogicValidation.EmailNotificationWithCollection("GestorImagenesXProducto", counter, collection);
                }
            }
        }

        public class ImageneXProductoDTO
        {
            public string ITEM_CODE { get; set; }
            public string ITEM_COLOR { get; set; }
            public string IMAGE_PATH { get; set; }
            public string IMAGE_MAIN { get; set; }

            public ImageneXProductoDTO(FotografiasXProducto imagen, string producto)
            {
                ITEM_CODE = producto;
                ITEM_COLOR = imagen.CodigoColor;
                IMAGE_PATH = imagen.FotografiaProducto;
                IMAGE_MAIN = (imagen.Principal == true) ? "1" : "0";
            }
        }

        public static bool EvaluarModelos(object imagenBD, object imagen)
        {
            if (LogicValidation.AreModelsValids(imagenBD, imagen))
            {
                return false;
            }

            if (LogicValidation.AreModelDistinct(imagenBD, imagen))
            {
                return false;
            }

            var properties = imagenBD.GetType().GetProperties();
            foreach (var property in properties)
            {
                var aPropValue = property.GetValue(imagenBD) ?? string.Empty;
                var bPropValue = property.GetValue(imagen) ?? string.Empty;
                if (aPropValue.ToString() != bPropValue.ToString())
                    return false;
            }
            return true;
        }

        public async Task<int> CreandoFotografia(FotografiasXProducto imagen, int idProducto)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                imagen.IdProducto = idProducto;
                imagen.Codigo = null;
                context.FotografiasXProducto.Add(imagen);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    contador++;
                    Console.WriteLine(ex);
                }
            }
            return contador;
        }
    }
}
