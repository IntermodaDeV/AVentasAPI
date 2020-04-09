using DBData.Database;
using DBData.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class AtributosXProductoRepository
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task SendToDatabase(List<AtributosxProducto> atributos, string CodigoProducto)
        {
            if (LogicValidation.ValidateDataCount(atributos.Count))
            {
                int updateCount = 0, insertCount = 0, errorCount = 0;
                using (AVentasEntities context = new AVentasEntities())
                {
                    foreach (var atributo in atributos)
                    {
                        if (LogicValidation.IsDataValid(atributo))
                        {
                            var productoXColeccion = await context.ProductosxColeccion.FirstOrDefaultAsync(prod => prod.CodigoProducto == CodigoProducto);
                            if (LogicValidation.IsDataValid(productoXColeccion))
                            {
                                var atributoBD = context.AtributosxProducto.FirstOrDefault(x => x.CodigoAtributo == atributo.CodigoAtributo
                                                 && x.IdProducto == productoXColeccion.IdProducto);
                                if (LogicValidation.IsDataValid(atributoBD))
                                {
                                    updateCount++;
                                    context.Entry(atributoBD).State = EntityState.Modified;
                                    atributoBD.CodigoAtributo = atributo.CodigoAtributo;
                                    atributoBD.IdProducto = productoXColeccion.IdProducto;
                                    atributoBD.Descripcion1 = atributo.Descripcion1;
                                    atributoBD.Descripcion2 = atributo.Descripcion2;

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
                                else
                                {
                                    insertCount++;
                                    errorCount += await CreandoAtributo(atributo, productoXColeccion.IdProducto);
                                }
                            }
                        }
                    }
                    string coleccion = "Id: " + CodigoProducto;
                    string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                    LogicValidation.EmailNotificationWithCollection("GestorAtributosXProductos", counter, coleccion);
                }
            }
        }

        public async Task<int> CreandoAtributo(AtributosxProducto atributo, int? idProducto)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                atributo.IdProducto = idProducto;
                context.AtributosxProducto.Add(atributo);

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
