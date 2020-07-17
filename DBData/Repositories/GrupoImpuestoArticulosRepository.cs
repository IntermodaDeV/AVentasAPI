using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class GrupoImpuestoArticulosRepository
    {
        public async Task<List<GrupoImpuestoArticulo>> ObtenerGrupoImpuestoArticulos()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.GrupoImpuestoArticulo.AsNoTracking().ToList();
            }
        }
        
        public async Task GuardarGrupoImpuestoArticulos(List<GrupoImpuestoArticulo> GrupoImpuestoArticulos)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.GrupoImpuestoArticulo.AddRange(GrupoImpuestoArticulos);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<GrupoImpuestoArticulo>> ModificarOAgregarGrupoImpuestoArticulos(List<GrupoImpuestoArticulo> GrupoImpuestoArticulos)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                var GrupoImpuestoArticulosEnDB = context.GrupoImpuestoArticulo;
                foreach (var ImpuestoArticulos in GrupoImpuestoArticulos)
                {
                    var ImpuestoArticulosEnDB = GrupoImpuestoArticulosEnDB.FirstOrDefault(col => col.GrupoProducto == ImpuestoArticulos.GrupoProducto && col.GrupoImpuesto == ImpuestoArticulos.GrupoImpuesto && col.Porcentaje == ImpuestoArticulos.Porcentaje);
                    if (ImpuestoArticulosEnDB == null)
                    {
                        context.GrupoImpuestoArticulo.Add(ImpuestoArticulos);
                    }
                    else
                    {
                        ImpuestoArticulosEnDB = new GrupoImpuestoArticulo();

                        ImpuestoArticulosEnDB.GrupoProducto = ImpuestoArticulos.GrupoProducto;
                        ImpuestoArticulosEnDB.GrupoImpuesto = ImpuestoArticulos.GrupoImpuesto;
                        ImpuestoArticulosEnDB.Porcentaje = ImpuestoArticulos.Porcentaje;
                        ImpuestoArticulosEnDB.Empresa = ImpuestoArticulos.Empresa;
                    }
                }
                await context.SaveChangesAsync();
                return GrupoImpuestoArticulosEnDB.AsNoTracking().ToList();
            }
        }
    }
}
