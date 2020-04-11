using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class LineasRepository
    {
        public async Task<List<MaestroLinea>> ObtenerLineas()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.MaestroLinea.AsNoTracking().ToList();

            }

        }
    }
}
