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

        [Route("~/api/TallasPorProducto")]
        [HttpGet()]
        public async Task<IHttpActionResult> SizesByProduct()
        {

            GestorSizesByProduct gestorSizesByProduct = new GestorSizesByProduct();           
            return await Task.FromResult(Ok());
        }
    }
}
