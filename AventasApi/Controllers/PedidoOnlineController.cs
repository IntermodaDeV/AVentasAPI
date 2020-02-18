using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AventasApi.GestorData;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;
//using IMS.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Responses;
using RestSharp;


namespace AventasApi.Controllers
{
    public class PedidoOnlineController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        const string MongoDBConnectionString = "mongodb://209.126.64.158:27017";
        const string MongoDBName = "Intermoda";
        const string PedidoTemporal = "PedidoTemporal";
        private MongoClient Client;
        private IMongoDatabase Database;

        public PedidoOnlineController()
        {

            Client = new MongoClient(MongoDBConnectionString);
            Database = Client.GetDatabase(MongoDBName);

            if (!BsonClassMap.IsClassMapRegistered(typeof(PedidoApiModel)))
            {
                BsonClassMap.RegisterClassMap<PedidoApiModel>(pam =>
                {
                    pam.AutoMap();
                    pam.SetIgnoreExtraElements(true);
                });
            }

        }
        [Route("api/PedidoOnline/CrearEncabezado/")]
        [HttpPost]
        public IHttpActionResult CrearEncabezado([FromBody] EncPedTempViewModel EncabezadoPedido)
        {
            PedidoApiModel encabezadoPedidoAX = new PedidoApiModel
            {
                userName = "desarrollo",
                password = "Intermoda2020",
                SalesTable = new SalesTable
                {
                    Cliente = EncabezadoPedido.CodigoCliente,
                    Linea = EncabezadoPedido.IdLinea,
                    AcuerdoVentaId = EncabezadoPedido.AcuerdoVenta,
                    Fecha = DateTime.Now,
                    FechaEntrega = DateTime.Now,
                    CodigoColeccion = EncabezadoPedido.CodigoColeccion,
                    TipoVenta = (EncabezadoPedido.TipoColeccion == "B") ? "SINLOTE" : "LOTE-CONFC",
                    TipoColeccion = EncabezadoPedido.TipoColeccion
                },
                Crear = EncabezadoPedido.Crear
            };
            var collection = Database.GetCollection<PedidoApiModel>(PedidoTemporal);
            var filter = Builders<PedidoApiModel>.Filter.Eq(pam => pam.SalesTable.Cliente, EncabezadoPedido.CodigoCliente);


            var client = new RestClient(@"http://190.109.223.244:8084/api/salesOrder/createEnc");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(encabezadoPedidoAX);
            IRestResponse<Response<string>> response = client.Execute<Response<string>>(request);
            string content = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);
            encabezadoPedidoAX.userName = null;
            encabezadoPedidoAX.password = null;
            CustomResopnse customResopnse = verificarPedidoExitoso(content);
            if (customResopnse.Error)
            {
                return BadRequest("Error al Guardar" + Environment.NewLine + customResopnse.Content);
            }
            encabezadoPedidoAX.SalesTable.Orden = customResopnse.Content;

            collection.ReplaceOne(filter, encabezadoPedidoAX, new UpdateOptions { IsUpsert = true });
            //var pedidoAx =
            //    collection.Find(filter).FirstOrDefault();
            //if (pedidoAx == null)
            //{
            //    collection.InsertOne(encabezadoPedidoAX);
            //}
            //else
            //{
            //    if (!(pedidoAx.SalesTable.Cliente == encabezadoPedidoAX.SalesTable.Cliente &&
            //          pedidoAx.SalesTable.Linea == encabezadoPedidoAX.SalesTable.Linea &&
            //          pedidoAx.SalesTable.CodigoColeccion == encabezadoPedidoAX.SalesTable.CodigoColeccion))
            //    {
            //        if (pedidoAx.SalesTable.Orden != null &&
            //            (pedidoAx.SalesTable.lineaOrden != null &&
            //             (pedidoAx.SalesTable.lineaOrden.parmSalesLineList != null &&
            //              pedidoAx.SalesTable.lineaOrden.parmSalesLineList.Count > 0)))
            //        {
            //            //cancelarPedido();
            //            //collection.InsertOne(encabezadoPedidoAX);

            //        }
            //    }
            //}


            return Ok(encabezadoPedidoAX);
        }
        [Route("api/PedidoOnline/CrearDetalle/")]
        [HttpPost]
        public IHttpActionResult CrearDetalle([FromBody] DetPedTempViewModel DetallePedido)
        {
            var collection = Database.GetCollection<PedidoApiModel>(PedidoTemporal);
            var filter = Builders<PedidoApiModel>.Filter.Eq(pam => pam.SalesTable.Cliente, DetallePedido.CodigoCliente);
            var pedidoMDB =
                collection.Find(filter).FirstOrDefault();

            LineaArticulo lineaArticulo = new LineaArticulo();
            if (pedidoMDB.SalesTable.lineaOrden == null || pedidoMDB.SalesTable.lineaOrden.parmSalesLineList == null || pedidoMDB.SalesTable.lineaOrden.parmSalesLineList.Count == 0)
            {
                if (DetallePedido.Cantidad == 0)
                {
                    return Ok();
                }
                lineaArticulo = new LineaArticulo
                {
                    Articulo = DetallePedido.ProductoId,
                    Color = DetallePedido.CodigoColor,
                    Talla = DetallePedido.Talla,
                    Cantidad = DetallePedido.Cantidad,
                    MontoLinea = DetallePedido.MontoLinea,
                    PrecioUnitario = DetallePedido.PrecioUnitario,
                    Linea = 1
                };
                pedidoMDB.SalesTable.lineaOrden = new LineaOrden
                {
                    parmSalesLineList = new List<LineaArticulo>()
                    {
                        lineaArticulo
                    }
                };
            }
            else
            {
                lineaArticulo = pedidoMDB.SalesTable.lineaOrden.parmSalesLineList.FirstOrDefault(lin => lin.Articulo == DetallePedido.ProductoId && lin.Color == DetallePedido.CodigoColor && lin.Talla == DetallePedido.Talla);
                if (lineaArticulo == null)
                {
                    if (DetallePedido.Cantidad == 0)
                    {
                        return Ok();
                    }
                    lineaArticulo = new LineaArticulo
                    {
                        Articulo = DetallePedido.ProductoId,
                        Color = DetallePedido.CodigoColor,
                        Talla = DetallePedido.Talla,
                        Cantidad = DetallePedido.Cantidad,
                        MontoLinea = DetallePedido.MontoLinea,
                        PrecioUnitario = DetallePedido.PrecioUnitario,
                        Linea = pedidoMDB.SalesTable.lineaOrden.parmSalesLineList.Count + 1,
                        Actualizar = true
                    };
                    pedidoMDB.SalesTable.lineaOrden.parmSalesLineList.Add(lineaArticulo);
                }
                else
                {


                    lineaArticulo.Eliminar = (DetallePedido.Cantidad == 0);
                    lineaArticulo.Actualizar = (DetallePedido.Cantidad > 0);

                    lineaArticulo.Cantidad = DetallePedido.Cantidad;
                }

            }

            if (pedidoMDB.Crear)
            {

                PedidoApiModel encabezadoPedidoAX = new PedidoApiModel
                {
                    userName = "desarrollo",
                    password = "Intermoda2020",
                    SalesTable = new SalesTable
                    {
                        Orden = pedidoMDB.SalesTable.Orden,
                        lineaOrden = new LineaOrden
                        {
                            parmSalesLineList = new List<LineaArticulo>()
                            {
                                lineaArticulo
                            }
                        },
                        AcuerdoVentaId = pedidoMDB.SalesTable.AcuerdoVentaId,


                    },
                    Crear = false
                };

                var client = new RestClient(@"http://190.109.223.244:8084/api/salesOrder/createDet");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddJsonBody(encabezadoPedidoAX);
                IRestResponse<Response<string>> response = client.Execute<Response<string>>(request);
                string content =Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);
                CustomResopnse customResopnse = verificarPedidoExitoso(content);
                if (customResopnse.Error)
                {
                    content = content.Remove(0, 5);
                    var collection2 = Database.GetCollection<PedidosFallidosViewModel>("PedidosFallidos");
                    collection2.InsertOne(new PedidosFallidosViewModel
                    {
                        Pedido = encabezadoPedidoAX,
                        Fecha = DateTime.Now,
                        MensajeAx = content
                    });
                    return BadRequest("Error al Guardar" + Environment.NewLine + customResopnse.Content);
                }
                collection.ReplaceOne(filter, pedidoMDB, new UpdateOptions { IsUpsert = true });

            }
            return Ok(pedidoMDB);
        }

        [Route("api/PedidoOnline/FinalizarPedido/")]
        [HttpPost]
        public IHttpActionResult FinalizarPedido([FromBody] EncPedTempViewModel EncabezadoPedido)
        {
            var collection = Database.GetCollection<PedidoApiModel>(PedidoTemporal);
            var filter = Builders<PedidoApiModel>.Filter.Eq(pam => pam.SalesTable.Cliente, EncabezadoPedido.CodigoCliente);

            var pedidoAx =
                collection.Find(filter).FirstOrDefault();
            if (pedidoAx != null && pedidoAx.SalesTable.Orden != null)
            {
                var client = new RestClient(@"http://190.109.223.244:8084/api/salesOrder/FinalizarPedido");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddJsonBody(new
                {
                    userName = "desarrollo",
                    password = "Intermoda2019",
                    companyId = "IMHN",
                    Orden = pedidoAx.SalesTable.Orden
                });
                IRestResponse<Response<string>> response = client.Execute<Response<string>>(request);
                string content = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);

            }
            // buscar el pedido en mongo y mandarlo a guardar a finalizar en ax en caso de que crear sea igual a true, de lo contrario solo solo mandar a ax el pedido que esta en mongo
            return Ok();
        }
        [Route("api/PedidoOnline/CancelarPedido/")]
        [HttpPost]
        public IHttpActionResult CancelarPedido([FromBody] EncPedTempViewModel EncabezadoPedido)
        {
            var collection = Database.GetCollection<PedidoApiModel>(PedidoTemporal);
            var filter = Builders<PedidoApiModel>.Filter.Eq(pam => pam.SalesTable.Cliente, EncabezadoPedido.CodigoCliente);

            var pedidoAx =
                collection.Find(filter).FirstOrDefault();
            if (pedidoAx != null && pedidoAx.SalesTable.Orden != null)
            {
                var client = new RestClient(@"http://190.109.223.244:8084/api/salesOrder/CancelarPedido");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddJsonBody(new
                {
                    userName = "desarrollo",
                    password = "Intermoda2019",
                    companyId = "IMHN",
                    Orden = pedidoAx.SalesTable.Orden
                });
                IRestResponse<Response<string>> response = client.Execute<Response<string>>(request);
                string content = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);

            }
            collection.DeleteOne(filter);
            // buscar el pedido en mongo y mandarlo a guardar a finalizar en ax en caso de que crear sea igual a true, de lo contrario solo solo mandar a ax el pedido que esta en mongo
            return Ok();
        }

        private CustomResopnse verificarPedidoExitoso(string content)
        {
            string responseContent = content;
            CustomResopnse customResopnse = new CustomResopnse();
            if (responseContent.StartsWith("true"))
            {
                responseContent = responseContent.Remove(0, 4);
                responseContent = responseContent.Split(' ')[0];
                customResopnse.Error = false;
            }
            else
            {
                customResopnse.Error = true;

            }
            customResopnse.Content = responseContent;

            return customResopnse;
        }
        private void cancelarPedido()
        {
            //cancelar el pedido en ax y borrarlo del mongo
        }
    }
    public class CustomResopnse
    {
        public string Content { get; set; }
        public bool Error { get; set; }
    }
}
