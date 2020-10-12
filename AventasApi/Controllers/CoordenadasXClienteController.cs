using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
//using AventasApi.GestorData;
//using AventasApi.Models.ApiModels;
using AventasApi.Models.CustomerLocationApp;
using DBData.Database;

namespace AventasApi.Controllers
{
    public class CoordenadasXClienteController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            string UrlString = @"http://209.126.64.158:3083/api/CoordenadasXCliente";
            HttpClient client = new HttpClient();

            List<RutaConCoordenadaViewModel> rutas = new List<RutaConCoordenadaViewModel>();
            HttpResponseMessage response = await client.GetAsync(UrlString).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                rutas = await response.Content.ReadAsAsync<List<RutaConCoordenadaViewModel>>();
                if (rutas != null && rutas.Count > 0)
                {

                    return Ok(rutas);
                }

                return Ok(new List<RutaConCoordenadaViewModel>());
            }
            return StatusCode(response.StatusCode);
        }

        [HttpGet]
        [Route("~/api/cliente/global")]
        public List<Coordenada> GetClientes()
        {
            try
            {
                var result = context.Clientes.Where(x => x.Longitud != null && x.Latitud != null).ToList();
                if (result.Count > 0)
                {
                    var clientes = result.Select(x => new Coordenada()
                    {
                        ACCOUNT = x.CodigoCliente,
                        NAME = x.Nombre,
                        LATITUDE = x.Latitud.Value,
                        LONGITUD = x.Longitud.Value,
                        COMPANY = x.EmpresaId
                    }).ToList();

                    return clientes;
                }
                return new List<Coordenada>();
            }
            catch(Exception e)
            {
                return new List<Coordenada>();
            }
        }
    }
}
