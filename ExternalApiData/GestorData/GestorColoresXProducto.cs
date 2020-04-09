using System;
using System.Collections.Generic;
using System.Linq;
using ExternalApiData.Utils;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Models.ApiModels;
using System.Diagnostics;
using DBData.Database;
using ExternalApiData.Enviroments;
using RestSharp;
using Newtonsoft.Json;

namespace ExternalApiData.GestorData
{
    public class GestorColoresXProducto
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}";
        private static LogicValidation LogicValidation = new LogicValidation();

        public static async Task ObtenerColoresXProducto()
        {
            List<Colecciones> colecciones = new List<Colecciones>();

            using (AVentasEntities context = new AVentasEntities())
            {
                colecciones = context.Colecciones.ToList();
            }

            foreach (var coleccion in colecciones)
            {
                string peticion = string.Format(UrlString, coleccion.CodigoColeccion);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);
                if (response.IsSuccessful)
                {
                    var colores = JsonConvert.DeserializeObject<List<ColorXProductoCRMApiModel>>(response.Content);
                    if (LogicValidation.ValidateDataCount(colores.Count))
                    {
                        int updateCount = 0, insertCount = 0, errorCount = 0;
                        Parallel.ForEach(colores, color =>
                        {
                            if (LogicValidation.IsDataValid(color))
                            {
                                using (AVentasEntities context = new AVentasEntities())
                                {
                                    var producto = context.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == color.PRODUCT
                                       && prod.IdColeccion == coleccion.IdColeccion);
                                    if (LogicValidation.IsDataValid(producto))
                                    {
                                        var colorBD = context.ColoresxProducto.FirstOrDefault(col => col.IdProducto == producto.IdProducto
                                                       && col.CodigoColor == color.COLORCODE);
                                        if (!LogicValidation.IsDataValid(colorBD))
                                        {
                                            insertCount++;
                                            errorCount += CreandoColor(color, producto.IdProducto);
                                        }
                                    }
                                }
                            }
                        });
                        string collection = "Id " + coleccion.IdColeccion + ", Cod " + coleccion.CodigoColeccion;
                        string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
                        LogicValidation.EmailNotificationWithCollection("GestorColoresXProducto", counter, collection);
                    }
                }
            }
        }

        public static int CreandoColor(ColorXProductoCRMApiModel color, int IdProducto)
        {
            int contador = 0;
            using (AVentasEntities context = new AVentasEntities())
            {
                ColoresxProducto nuevoColor = new ColoresxProducto()
                {
                    CodigoColor = color.COLORCODE,
                    IdProducto = IdProducto,
                    Disponible = null
                };
                context.ColoresxProducto.Add(nuevoColor);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception) { contador++; }
            }
            return contador;
        }
    }
}