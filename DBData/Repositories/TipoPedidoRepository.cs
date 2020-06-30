using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBData.Repositories
{
    public class TipoPedidoRepository
    {
        public async Task<List<TiposdePedido>> ObtenerTiposdePedido()
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                return context.TiposdePedido.AsNoTracking().ToList();
            }
        }

    }
}
