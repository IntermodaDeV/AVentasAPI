using AventasApi.GestorData;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class TallasPorProductoController : ApiController
    {
        AVentasEntities _context = new AVentasEntities();
        //private readonly AuthenticationAppService _authenticationAppService;

        //public TallasPorProductoController()
        //{
        //    _authenticationAppService = new AuthenticationAppService();
        //}

        [Route("~/api/TallasPorProducto/{codColeccion}")]
        [HttpGet()]
        public async Task<IHttpActionResult> SizesByProduct(string codColeccion)
        {
            var validateParameter = codColeccion == null;
            if (validateParameter)
            {
                return BadRequest();
            }

            GestorSizesByProduct gestorSizesByProduct = new GestorSizesByProduct();           
            bool result = await gestorSizesByProduct.ObtenerTallasXProducto(codColeccion);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
