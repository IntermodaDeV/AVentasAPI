using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    public class GeoposicionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpPost]
        public IHttpActionResult Post([FromBody] GoeposicionXAsesorViewModel geoposicion)
        {
            context.BitacoraGeoposicion.Add(new BitacoraGeoposicion
            {
                IdAsignacionxAsesor=geoposicion.IdAsignacionxAsesor,
                Mocked=geoposicion.Mocked,
                Accuracy=geoposicion.Accuracy,
                Altitude=geoposicion.Altitude,
                Latitude=geoposicion.Latitude,
                Longitude=geoposicion.Longitude,
                CodigoAsesor=geoposicion.CodigoAsesor,
                Fecha = DateTime.Now
            });
            context.SaveChanges();
            return Ok();
        }
    }
}
