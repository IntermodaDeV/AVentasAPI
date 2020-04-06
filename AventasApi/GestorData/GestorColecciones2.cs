using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using DBData.Database;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using Newtonsoft.Json;
using RestSharp;

namespace AventasApi.GestorData
{
    public class GestorColecciones2
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}api/paquetes/imhn/";
        public async Task<ColeccionesYTiposDeColeccionViewModel> ObtenerColecciones()
        {
            List<Colecciones> ColeccionesAGuardar = new List<Colecciones>();
            List<TiposdeColeccion> tiposdeColeccionAGuardar = new List<TiposdeColeccion>();
            var restClient = new RestClient(UrlString);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                var colecciones = JsonConvert.DeserializeObject<List<ColeccionCRMApiModel>>(response.Content);
                foreach (var coleccion in colecciones)
                {
                    if (coleccion.PACKAGE_TYPE != "N/A")
                    {
                        Colecciones coleccionAGuardar = new Colecciones
                        {
                            CodigoColeccion = coleccion.PACKAGE,
                            Nombre = coleccion.NAME,
                            ColeccionTipo = coleccion.PACKAGE_TYPE,
                            EmpresaId = coleccion.ENTITY,
                            DisenoInicio = DateTime.ParseExact(coleccion.START_DATE_DESIGN, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            DisenoFinal = DateTime.ParseExact(coleccion.END_DATE_DESIGN, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            EntregaInicio = DateTime.ParseExact(coleccion.START_DATE_DELIVERY_SALES_ORDER, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            EntregaFinal = DateTime.ParseExact(coleccion.END_DATE_DELIVERY_SALES_ORDER, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            ProduccionInicio = DateTime.ParseExact(coleccion.START_DATE_PRODUCTION, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            ProduccionFinal = DateTime.ParseExact(coleccion.END_DATE_PRODUCTION, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            VentaInicio = DateTime.ParseExact(coleccion.START_DATE_SALES_ORDER_ENTRY, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            VentaFinal = DateTime.ParseExact(coleccion.END_DATE_SALES_ORDER_ENTRY, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            Estatus = int.Parse(coleccion.STATUS),
                            FotoPortada = @"https://aventas.devcit.com:3044/ImagenesXProducto/portadacoleccion.jpg",
                        };
                        TiposdeColeccion tipoDeColeccionAGuardar = new TiposdeColeccion
                        {
                            ColeccionTipo = coleccion.PACKAGE_TYPE,
                            Descripcion = coleccion.PACKAGE_TYPE_NAME
                        };
                        if (!tiposdeColeccionAGuardar.Any(tp => tp.ColeccionTipo == tipoDeColeccionAGuardar.ColeccionTipo))
                            tiposdeColeccionAGuardar.Add(tipoDeColeccionAGuardar);
                        ColeccionesAGuardar.Add(coleccionAGuardar);

                    }
                }
            }
            return new ColeccionesYTiposDeColeccionViewModel
            {
                Colecciones = ColeccionesAGuardar,
                TiposdeColeccion = tiposdeColeccionAGuardar
            };
        }

        public async Task GuardarColecciones(List<Colecciones> coleccionesAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Colecciones.AddRange(coleccionesAGuardar);
                await context.SaveChangesAsync();
            }
        }
        public async Task GuardarTiposDeColecciones(List<TiposdeColeccion> tiposDeColeccionAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.TiposdeColeccion.AddRange(tiposDeColeccionAGuardar);
                await context.SaveChangesAsync();
            }
        }
    }
}