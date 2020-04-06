using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    public class GeoposicionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public GeoposicionController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpPost]
        public IHttpActionResult Post([FromBody] GoeposicionXAsesorViewModel geoposicion)
        {
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

            context.BitacoraGeoposicion.Add(new BitacoraGeoposicion
            {
                IdAsignacionxAsesor=geoposicion.IdAsignacionxAsesor,
                Mocked=geoposicion.Mocked,
                Accuracy=geoposicion.Accuracy,
                Altitude=geoposicion.Altitude,
                Latitude=geoposicion.Latitude,
                Longitude=geoposicion.Longitude,
                CodigoAsesor=user.UserAccount,
                Fecha = DateTime.Now
            });
            context.SaveChanges();
            return Ok();
        }
    }
}
