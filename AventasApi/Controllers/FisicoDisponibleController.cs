using AventasApi.Filters;
using DBData.Database;
using AventasApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
//using IMS.Tokens.Services;
using AventasApi.Models.Authentication;
//using AventasApi.Models.ApiModels;
//using AventasApi.GestorData;
using System;
//using AventasApi.Enviroments;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
//using IMS.Extensions;

namespace AventasApi.Controllers
{
    public class FisicoDisponibleController : ApiController
    {
        [Route("api/FisicoDisponible/")]
        [HttpGet]
        public async Task<IHttpActionResult> ActualizarCuentaCorriente(string ProductoId, string CodigoColor ,string CodigoTalla)
        {
            string UrlString = $"{Enviroment.KREAWebServiceURLApi}collection/disponibleEspecifico";
            HttpClient client = new HttpClient();
            var Credentials = new Dictionary<string, string> {
                { "userName", "desarrollo" },
                { "password", "Intermoda2020" },
                { "ItemID", ProductoId },
                { "color", CodigoColor },
                { "talla", CodigoTalla },
            };
            List<FisicoDisponibleXProductoApiModel> fisicosDisponibles = new List<FisicoDisponibleXProductoApiModel>();
            var content = new FormUrlEncodedContent(Credentials);
            HttpResponseMessage response = await client.PostAsync(UrlString, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                fisicosDisponibles = await response.Content.ReadAsAsync<List<FisicoDisponibleXProductoApiModel>>();
                if (fisicosDisponibles != null && fisicosDisponibles.Count > 0)
                {

                return Ok(new {fisicaDisponible = fisicosDisponibles[0].fisicaDisponible});
                }

                return BadRequest("No se encontro datos para los parametros proporcionados.");
            }

            return BadRequest( Newtonsoft.Json.JsonConvert.SerializeObject(response.Content));



        }
    }
}
