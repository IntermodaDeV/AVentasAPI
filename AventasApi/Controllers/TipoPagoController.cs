using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class TipoPagoController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetTiposPago()
        {

            var tiposPago = context.TiposdePago.Select(tipoPago => new TipoPagoViewModel
            {
                IdTipoPago = tipoPago.IdTipoPago,
                Codigo = tipoPago.Codigo,
                Descripcion = tipoPago.Descripcion,
                Tipo = tipoPago.Tipo,
                EmpresaId = tipoPago.EmpresaId,
                TiposdePagoDetalle = tipoPago.TiposdePagoDetalle.Select(tipoPagoDetalle=> new TipoPagoDetalleViewModel{
                    IdTipoPagoDetalle = tipoPagoDetalle.IdTipoPagoDetalle,
                    Codigo = tipoPagoDetalle.Codigo,
                    CodigoDetalle = tipoPagoDetalle.CodigoDetalle,
                    Descripcion = tipoPagoDetalle.Descripcion,
                    EmpresaId = tipoPagoDetalle.EmpresaId,
                    IdTipoPago = tipoPagoDetalle.IdTipoPago,
                }).ToList(),
            }).ToList();
            return Ok(tiposPago);
        }
    }
}
