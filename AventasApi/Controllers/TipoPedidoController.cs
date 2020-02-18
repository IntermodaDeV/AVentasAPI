using AventasApi.GestorData;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace AventasApi.Controllers
{
    public class TipoPedidoController : ApiController
    {
        AVentasEntities context = new AVentasEntities();


        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            List<TipoPedidoViewModel> pedidos = context.TiposdePedido.Select(tp => new TipoPedidoViewModel
            {
                IdTipoPedido = tp.IdTipoPedido,
                TipoPedido = tp.TipoPedido,
                HabilitaEstilos = tp.HabilitaEstilos??false,
                Imagen = tp.Url_Imagen,
                Aplica_Todos = tp.Aplica_Todos??false,
                Restrictivo = tp.Restrictivo??false
            }).ToList();

            return Ok(pedidos);
        }
    }
}
