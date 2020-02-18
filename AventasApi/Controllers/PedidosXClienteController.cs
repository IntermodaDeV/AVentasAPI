using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AventasApi.Filters;
using AventasApi.GestorData;
using AventasApi.Infrastructure;
using AventasApi.Models.ApiModels;
using AventasApi.Models.Authentication;
using AventasApi.Models.ViewModels;
//using IMS.Extensions;
using IMS.Tokens.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Responses;
using RestSharp;
using System.Data.Entity;
using AventasApi.Services.AsyncJobs;
using AventasApi.Models;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    //[Auth]
    public class PedidosXClienteController : ApiController
    {
        //public static HttpClient client = new ClienteHttp();
        const string MongoDBConnectionString = "mongodb://209.126.64.158:27017";
        const string MongoDBName = "Intermoda";
        const string PedidoTemporal = "PedidosFallidos";
        private MongoClient Client;
        private IMongoDatabase Database;
        private readonly AuthenticationAppService _authenticationAppService;

        public PedidosXClienteController()
        {
            _authenticationAppService = new AuthenticationAppService();

            Client = new MongoClient(MongoDBConnectionString);
            Database = Client.GetDatabase(MongoDBName);

            if (!BsonClassMap.IsClassMapRegistered(typeof(PedidosFallidosViewModel)))
            {
                BsonClassMap.RegisterClassMap<PedidosFallidosViewModel>(pam =>
                {
                    pam.AutoMap();
                    pam.SetIgnoreExtraElements(true);
                });
            }

        }

        public IHttpActionResult Get()
        {
            using (AVentasEntities context = new AVentasEntities())
            {

                List<PedidosXClienteViewModel> pedidos = context.PedidosxCliente.OrderByDescending(ped => ped.PedidoId).Select(ped => new PedidosXClienteViewModel
                {
                    PedidoId = ped.PedidoId,
                    NombreColeccion = context.Colecciones.FirstOrDefault(col => col.IdColeccion == ped.IdColeccion).Nombre,
                    TotalUnidades = ped.TotalUnidades,
                    TotalXPedido = ped.TotalPedido,
                    SubTotalXPedido = ped.Subtotal,
                    Impuesto = ped.TotalImpuesto,
                    Cliente = new ClienteViewModel
                    {
                        Codigo = ped.Clientes.CodigoCliente,
                        Nombre = ped.Clientes.Nombre,
                        Direccion = ped.Clientes.Direccion,
                        Moneda = ped.Clientes.IdMoneda
                    },
                    Linea = context.MaestroLinea.Select(ml => new LineaViewModel
                    {
                        IdLinea = ml.IdLinea,
                        Linea = ml.Linea,
                    }).FirstOrDefault(ml => ml.IdLinea == ped.IdLinea),
                    TipoPedido = context.TiposdePedido.Select(tp => new TipoPedidoViewModel
                    {
                        IdTipoPedido = tp.IdTipoPedido,
                        TipoPedido = tp.TipoPedido,
                        HabilitaEstilos = tp.HabilitaEstilos ?? false,
                        Imagen = tp.Url_Imagen,
                        Aplica_Todos = tp.Aplica_Todos ?? false,
                        Restrictivo = tp.Restrictivo ?? false
                    }).FirstOrDefault(tp => tp.IdTipoPedido == ped.IdTipoPedido),
                    AcuerdoVenta = ped.AcuerdoVenta,
                    EmpresaId = ped.EmpresaId,
                    FechaActual = ped.Fecha,
                    Usuario = context.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Usuario,
                    FechaEntrega = ped.FechaEntrega,
                    Observacion = ped.Observacion,
                    location = new Location
                    {
                        mocked = ped.Mocked ?? false,
                        accuracy = ped.Accuracy,
                        altitude = ped.Altitude,
                        latitude = ped.Latitude,
                        longitude = ped.Longitude,
                        error = ped.Error
                    },
                    gruposXDetPed = ped.PedidosDetalle.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
                        .Select(gruposXDetPed => new GruposTallaXDetPed
                        {
                            GrupoTalla = gruposXDetPed.Key,
                            ListaTalla = context.TallasXGrupo.Where(txp => txp.CodigoGrupoTalla == gruposXDetPed.Key).Select(txp => new TallaViewModel
                            {
                                GrupoTallaId = txp.CodigoGrupoTalla,
                                Talla = txp.CodigoTalla,
                                Orden = txp.Orden ?? 0
                            }).OrderBy(txp => txp.Orden).ToList(),
                            prodsXDetPed = gruposXDetPed.GroupBy(pedDet => pedDet.CodigoProducto)
                        .Select(pedDet => new ProductosXDetPed
                        {
                            IdProducto = pedDet.Key,
                            CodigoProducto = pedDet.FirstOrDefault().ProductosxColeccion.CodigoProducto,
                            NombreProducto = pedDet.FirstOrDefault().ProductosxColeccion.NombreProducto,
                            Imagen = pedDet.FirstOrDefault().ProductosxColeccion.FotografiasXProducto.FirstOrDefault().FotografiaProducto,
                            CantidadXProducto = pedDet.Sum(cant => cant.Cantidad),
                            TotalXProducto = pedDet.Sum(cant => cant.MontoLinea),
                            coloresXProdXDetPed = pedDet.GroupBy(colXprod => colXprod.CodigoColor).Select(colXprod =>
                                 new ColoresXProdXDetPed
                                 {
                                     CantidadXColor = colXprod.Sum(cant => cant.Cantidad),
                                     TotalXColor = colXprod.Sum(cant => cant.MontoLinea),
                                     PrecioXColor = colXprod.FirstOrDefault().PrecioUnitario,
                                     IdColor = colXprod.Key,
                                     NombreColor = context.Colores.FirstOrDefault(color => color.CodigoColor == colXprod.Key).Color,
                                     DetallesXPedido = colXprod.Select(detPed => new DetalleXPedidoViewModel
                                     {
                                         IdRegistro = detPed.IdPedidoDetalle,
                                         PedidoId = detPed.PedidoId,
                                         Cantidad = detPed.Cantidad,

                                         Linea = detPed.Linea,
                                         MontoLinea = detPed.MontoLinea,
                                         PrecioUnitario = detPed.PrecioUnitario,
                                         Talla = detPed.CodigoTalla
                                     }).ToList()

                                 }).ToList()
                        }).ToList()
                        }).ToList()

                }).ToList();
                foreach (var pedido in pedidos)
                {
                    string imagenB64 = "";

                    var firma = context.FirmasxPedido.FirstOrDefault(fir => pedido.PedidoId == fir.PedidoId);
                    if (firma != null)
                    {
                        try
                        {
                            imagenB64 = "data:image/png;base64," + Convert.ToBase64String(firma.Firma);
                        }
                        catch (Exception e)
                        {

                        }

                        pedido.Firma = imagenB64;

                    }
                }
                return Ok(pedidos);
            }

        }

        //[HttpGet]
        //public IHttpActionResult GetImagen(string id)
        //{
        //    string imagenB64 = "";
        //    using (AVentasEntities context = new AVentasEntities())
        //    {
        //        var firma = context.FirmasxPedido.FirstOrDefault();
        //        if (firma != null)
        //        {
        //            try
        //            {
        //                imagenB64 = "data:image/png;base64," + Convert.ToBase64String(firma.Firma);
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }
        //    }
        //    return Ok(imagenB64);
        //}

        [HttpPost]
        public IHttpActionResult Post([FromBody] PedidoPostViewModel Pedido)
        {
            var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
            //var user = new { UserAccount = "gmonrroy" };
            Asesores asesor;
            Colecciones coleccion;
            AcuerdosxCliente acuerdoVenta;
            TiposdePedido tipoPedido;
            Clientes cliente;
            using (AVentasEntities context = new AVentasEntities())
            {
                asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                coleccion = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().FirstOrDefault(col => col.CodigoColeccion == Pedido.CodigoColeccion);
                acuerdoVenta = context.AcuerdosxCliente.Include(acu => acu.TiposdePedido).AsNoTracking().FirstOrDefault(acu => acu.IdAcuerdoxCliente == Pedido.AcuerdoVenta);
                tipoPedido = acuerdoVenta?.TiposdePedido;
                cliente = context.Clientes.AsNoTracking().FirstOrDefault(cli => cli.CodigoCliente == Pedido.CodigoCliente);
            }
            DateTime fechaEntrega = (Pedido.FechaEntrega.HasValue) ? Pedido.FechaEntrega.Value : DateTime.Now;
            PedidosxCliente PedidoBDAGuardar = new PedidosxCliente
            {
                //PedidoId = ,
                IdTipoPedido = tipoPedido?.IdTipoPedido,
                IdColeccion = coleccion.IdColeccion,
                CodigoCliente = cliente.CodigoCliente,
                AcuerdoVenta = acuerdoVenta?.IdAcuerdoxCliente,
                EmpresaId = cliente.EmpresaId,
                Fecha = DateTime.Now,
                FechaEntrega = fechaEntrega,
                CodigoAsesor = asesor.CodigoAsesor,
                Observacion = Pedido.Observacion,
                TotalUnidades = 0,
                PedidosDetalle = new List<PedidosDetalle>(),
                Subtotal = 0,
                //TotalImpuesto = ,
                //TotalDescuento = ,
                //TotalPedido = ,
                //Mocked = ,
                //Accuracy = ,
                //Altitude = ,
                //Latitude = ,
                //Longitude = ,
                //Error = ,
                IdLinea = Pedido.Linea,
                //idMoneda = ,
                //IdEstado = ,
            };

            int numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
            string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);

            string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";
            var pe = new PedidoCRMApiModel
            {
                COMPANY = "IMHN",
                CUSTOMER_ACCOUNT = Pedido.CodigoCliente,
                DATE_CONFIRMED_RECEIPT = fechaEntrega.ToString("dd/MM/yyyy"),
                DELIVERY_ADDRESS = "",
                DELIVERY_MODE = "",
                DISC_GROUP = "",
                ID_SALES_AGREEMENT = Pedido.AcuerdoVenta,
                LINE = Pedido.Linea,
                OBSERVATIONS = Pedido.Observacion,
                PACKAGE = coleccion.CodigoColeccion,
                PACKAGE_TYPE = coleccion.ColeccionTipo,
                PedidoJsonItems = new List<PedidoJsonItems>(),
                REFERENCE = numeroReferencia,
                SALES_MANAGER = asesor.Usuario,
                SALES_ORDER_TYPE = (coleccion.ColeccionTipo == "B") ? "SINLOTE" : "LOTE-CONFC",
                USER = asesor.Usuario,
            };
            foreach (var detalle in Pedido.DetallePedido)
            {
                int cantidad = 0;
                int.TryParse(detalle.Cantidad, out cantidad);
                if (cantidad > 0)
                {
                    pe.PedidoJsonItems.Add(
                            new PedidoJsonItems
                            {
                                COLOR = detalle.CodigoColor,
                                DELIVERY_ADDRESS = "",
                                DISC_PERCENTAGE = "0.00",
                                ITEM_CODE = detalle.CodigoProducto,
                                LOT_NUMBER = coleccion.CodigoColeccion,
                                QUANTITY = detalle.Cantidad,
                                REFERENCE = numeroReferencia,
                                SIZE = detalle.Talla,
                                UNIT = "Und",
                                UNIT_PRICE = detalle.PrecioUnitario
                            });
                    PedidoBDAGuardar.TotalUnidades += cantidad;
                    decimal precioUnitario = 0;
                    decimal.TryParse(detalle.PrecioUnitario, out precioUnitario);
                    PedidoBDAGuardar.Subtotal += (precioUnitario * cantidad);
                    PedidoBDAGuardar.PedidosDetalle.Add(new PedidosDetalle
                    {
                        CodigoProducto = coleccion.ProductosxColeccion.FirstOrDefault(prod => prod.CodigoProducto == detalle.CodigoProducto).IdProducto,
                        CodigoColor = detalle.CodigoColor,
                        CodigoTalla = detalle.Talla,
                        Cantidad = cantidad,
                        MontoLinea = (precioUnitario * cantidad),
                        Fecha = DateTime.Now,
                        CodigoAsesor = asesor.CodigoAsesor,
                        PrecioUnitario = precioUnitario
                    });
                }

            }


            PedidoBDAGuardar.TotalImpuesto = PedidoBDAGuardar.Subtotal.Value * decimal.Parse("0.15");
            PedidoBDAGuardar.TotalPedido = PedidoBDAGuardar.Subtotal.Value * decimal.Parse("1.15");
            string PEdidoID = "";
            try
            {
                var client = new RestClient(@"http://190.109.223.244:8083/api/pedidos/upload");
                client.Timeout = 480 * (1000);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddJsonBody(pe);
                IRestResponse<Response<string>> response = client.Execute<Response<string>>(request);
                string content = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);
                if (content.StartsWith("Success"))
                {
                    content = content.Remove(0, 8);
                    content = content.Split(' ')[0];
                    PEdidoID = content;
                    PedidoBDAGuardar.PedidoId = PEdidoID;
                }
                else
                {
                    return BadRequest(content);
                    //Random random = new Random();
                    //pedidoAGuardar.EncabezadoPedido.PedidoId = random.Next(1000).ToString();
                }

            }
            catch (Exception e)
            {
                return BadRequest(Newtonsoft.Json.JsonConvert.SerializeObject(e));
            }

            using (AVentasEntities context = new AVentasEntities())
            {
                asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                asesor.CorrelativoPedidos = numeroCorelativo + 1;
                context.SaveChanges();
            }
            AsyncSqlInsert.IngresarPedido(PedidoBDAGuardar,Pedido.Firma);

            return Ok(new { EncabezadoPedido = new { PedidoId = PEdidoID } });
        }



    }
}
