using DBData.Database;
using DBData.Utils;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class SizesByProductRepository
    {
        private static readonly LogicValidation logicValidation = new LogicValidation();

        public async Task SendToDatabase(List<TallaXProductoCRMApiModel> tallas)
        {
            if (logicValidation.ValidateDataCount(tallas.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                foreach (var talla in tallas)
                {
                    if (logicValidation.IsDataValid(talla))
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            int productoId = 0; int tallaId = 0;

                            var producto = await context.ProductosxColeccion
                                 .FirstOrDefaultAsync(x => x.CodigoProducto == talla.PRODUCT);
                            productoId = (producto == null) ? 0 : producto.IdProducto;

                            var grupoTalla = await context.TallasXGrupo
                                 .FirstOrDefaultAsync(x => x.CodigoGrupoTalla == talla.SIZEGROUP &&
                                 x.CodigoTalla == talla.SIZE);
                            tallaId = (grupoTalla == null) ? 0 : grupoTalla.IdTallaxGrupo;

                            var tallaBD = await context.TallasxProducto.FirstOrDefaultAsync(tal => tal.IdProducto == productoId
                                            && tal.IdTallaxGrupo == tallaId);
                            if (!logicValidation.IsDataValid(tallaBD))
                            {
                                insertCount++;
                                errorCount += await CreandoTallaXProducto(productoId, tallaId);
                            }
                        }
                    }
                }
                string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                logicValidation.EmailNotification("GestorSizesByProduct", counter);
            }
        }

        public async Task<int> CreandoTallaXProducto(int? productoId, int? tallaId)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                TallasxProducto nuevaTalla = new TallasxProducto()
                {
                    IdProducto = productoId,
                    IdTallaxGrupo =  tallaId
                };
                context.TallasxProducto.Add(nuevaTalla);

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
