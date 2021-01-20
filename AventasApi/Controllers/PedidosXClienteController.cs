using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestSharp;
using System.Data.Entity;
using AventasApi.Services.AsyncJobs;
using AventasApi.Models;
using AventasApi.Services.Authentication;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using Newtonsoft.Json;
using ExternalApiData.ApiModels;
using System.Threading.Tasks;

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

        private bool EnLinea(string empresa,string asesor)
        {
            var client = new RestClient(Enviroment.CRMWebServiceURLApi);
            client.Authenticator = new RestSharp.Authenticators.NtlmAuthenticator();
            var request = new RestRequest($"asesor/{empresa}/{asesor}", Method.GET);
            client.Timeout = 6000;
            IRestResponse<List<AsesorApiModel>> respuesta = client.Execute<List<AsesorApiModel>>(request);

            return respuesta.IsSuccessful;
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] PedidoPostViewModel Pedido)
        {
            try
            {
                string numeroReferencia = Pedido.NumeroReferencia;
                int numeroCorelativo = 0;
                var cache = false;
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                Asesores asesor;
                Colecciones coleccion;
                AcuerdosxCliente acuerdoVenta;
                TiposdePedido tipoPedido;
                Clientes cliente;
                ClienteContado clienteContado;
                CONFIGURACIONE SyncTelContado;
                CONFIGURACIONE SyncTelCredito;

                using (AVentasConfigEntities config = new AVentasConfigEntities())
                {
                    SyncTelContado = config.CONFIGURACIONES.FirstOrDefault(x => x.CODIGO == 1201);
                    SyncTelCredito = config.CONFIGURACIONES.FirstOrDefault(x => x.CODIGO == 1202);
                }

                using (AVentasEntities context = new AVentasEntities())
                {
                    clienteContado = context.ClienteContado.Find(Pedido.ClienteContadoId);
                    asesor = context.Asesores.AsNoTracking().FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                    acuerdoVenta = context.AcuerdosxCliente.Include(acu => acu.TiposdePedido).AsNoTracking().FirstOrDefault(acu => acu.IdAcuerdoxCliente == Pedido.AcuerdoVenta);
                    tipoPedido = acuerdoVenta?.TiposdePedido;
                    cliente = context.Clientes.AsNoTracking().FirstOrDefault(cli => cli.CodigoCliente == Pedido.CodigoCliente);
                    coleccion = context.Colecciones.Include(col => col.ProductosxColeccion).AsNoTracking().FirstOrDefault(col => col.CodigoColeccion == Pedido.CodigoColeccion && col.EmpresaId == cliente.EmpresaId);
                }
                DateTime fechaEntrega = (Pedido.FechaEntrega.HasValue) ? Pedido.FechaEntrega.Value : DateTime.Now;
                PedidosxCliente PedidoBDAGuardar = new PedidosxCliente
                {
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
                    Latitude = (Pedido.location != null) ? Pedido.location.latitude : null,
                    Longitude = (Pedido.location != null) ? Pedido.location.longitude : null,
                    IdLinea = Pedido.Linea,
                    ClienteContadoId = Pedido.ClienteContadoId,
                    ModoVenta = Pedido.ModoVenta,
                    Flete = Pedido.Flete,
                    RequiereEntrega = Pedido.RequiereEntrega
                };

                if (numeroReferencia == "")
                {
                    cache = true;
                    numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";
                }
                /*var pe = new PedidoCRMApiModel
                {
                    COMPANY = cliente.EmpresaId,
                    CUSTOMER_ACCOUNT = Pedido.CodigoCliente,
                    DATE_CONFIRMED_RECEIPT = fechaEntrega.ToString("dd/MM/yyyy"),
                    DELIVERY_ADDRESS = string.Empty,
                    DELIVERY_MODE = "",
                    DISC_GROUP = "",
                    ID_SALES_AGREEMENT = Pedido.AcuerdoVenta,
                    LINE = Pedido.Linea,
                    OBSERVATIONS = (Pedido.Observacion == null) ? "" : Pedido.Observacion,
                    PACKAGE = coleccion.CodigoColeccion,
                    PACKAGE_TYPE = coleccion.ColeccionTipo,
                    PedidoJsonItems = new List<PedidoJsonItems>(),
                    REFERENCE = numeroReferencia,
                    SALES_MANAGER = asesor.Usuario,
                    SALES_ORDER_TYPE = (coleccion.ColeccionTipo == "B") ? "SINLOTE" : "LOTE-CONFC",
                    USER = asesor.Usuario,
                    INCLUDE_TAX = "0"
                };

                if (clienteContado != null)
                {
                    pe.SALES_NAME = clienteContado.Nombre;
                    pe.FISCAL_DOCUMENT = clienteContado.RTN;
                    pe.DELIVERY_ADDRESS = clienteContado.Direccion;
                    pe.PHONE = (SyncTelContado.VALOR == "1") ? clienteContado.Telefono : "";
                }
                else
                {
                    pe.SALES_NAME = "";
                    pe.FISCAL_DOCUMENT = "";
                    pe.DELIVERY_ADDRESS = "";
                    pe.PHONE = (SyncTelCredito.VALOR == "1") ? cliente.Telefono : "";
                }*/

                foreach (var detalle in Pedido.DetallePedido)
                {
                    int cantidad = 0;
                    int.TryParse(detalle.Cantidad, out cantidad);
                    if (cantidad > 0)
                    {
                        /*pe.PedidoJsonItems.Add(
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
                                });*/
                        PedidoBDAGuardar.TotalUnidades += cantidad;
                        decimal precioUnitario = 0;
                        decimal.TryParse(detalle.PrecioUnitario, out precioUnitario);
                        PedidoBDAGuardar.Subtotal += (precioUnitario * cantidad);

                        PedidoBDAGuardar.PedidosDetalle.Add(new PedidosDetalle
                        {
                            IdProducto = detalle.IdProducto,
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

                PedidoBDAGuardar.TotalImpuesto = Pedido.Impuesto;
                PedidoBDAGuardar.TotalPedido = (PedidoBDAGuardar.Subtotal.Value + decimal.Parse(Pedido.Impuesto.ToString())) + Pedido.Flete;
                PedidoBDAGuardar.PedidoId = numeroReferencia;
                PedidoBDAGuardar.NumeroPedido = "";
                PedidoBDAGuardar.Sincronizado = false;
                PedidoBDAGuardar.Procesando = false;

                PResumenCredito_Result resultado;
                using (AVentasEntities context = new AVentasEntities())
                {
                    if (cache)
                    {
                        asesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount);
                        asesor.CorrelativoPedidos = numeroCorelativo + 1;
                        context.SaveChanges();
                    }
                    resultado = context.PResumenCredito().FirstOrDefault(x => x.codigocliente == cliente.CodigoCliente && x.Tipo == "Ordinario");
                }
                AsyncSqlInsert.IngresarPedido(PedidoBDAGuardar, Pedido.Firma);

                if (PedidoBDAGuardar.TotalPedido < resultado.Disponible)
                {
                    if (cliente.FacturacionEntrega.ToUpper() == "NO" || cliente.FacturacionEntrega.ToUpper() == "NUNCA")
                    {
                        ReducirStock(PedidoBDAGuardar);
                    }
                }

                //s_ = PostPedidoPendiente(numeroReferencia);

                return Ok(numeroReferencia);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PedidosXCliente/correlativo")]
        public async Task <IHttpActionResult> GetCorrelativo()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount);
                    int numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                    string inicialesAsesor = asesor.Nombre.Split(' ').Aggregate("", (iniacialesAcumuladas, nombreSiguiente) => iniacialesAcumuladas + nombreSiguiente[0]);
                    string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";

                    var toUpdate = ctx.Asesores.FirstOrDefault(x => x.CodigoAsesor == asesor.CodigoAsesor);
                    if (toUpdate != null)
                    {
                        toUpdate.CorrelativoPedidos = numeroCorelativo + 1;
                        ctx.SaveChanges();
                    }

                    return Ok(numeroReferencia);
                }
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("~/api/PedidosXCliente/{asesor}/{FechaInicio}/{FechaFin}")]
        public async Task<IHttpActionResult> Get(string Asesor, DateTime FechaInicio, DateTime FechaFin)
        {
            try {
                using (AVentasEntities context = new AVentasEntities())
                {

                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await context.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await context.Asesores.Where(x => x.CodigoAsesor == Asesor && x.Activo==true).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await context.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await context.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo==true).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    List<PedidosXClienteViewModel> ListaPedidos = new List<PedidosXClienteViewModel>();
                    foreach (var asesor in asesoresHabilitados)
                    {
                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }

                    List<PedidosXClienteViewModel> pedidos = context.PedidosxCliente.Where(p => p.CodigoAsesor == asesor && p.Fecha >= FechaInicio && p.Fecha < FechaFin).OrderByDescending(ped => ped.PedidoId).Select(ped => new PedidosXClienteViewModel
                    {
                        Asesor = ped.CodigoAsesor,
                        PedidoId = ped.PedidoId,
                        NumeroPedido = ped.NumeroPedido,
                        Sincronizado = ped.Sincronizado,
                        NombreColeccion = context.Colecciones.FirstOrDefault(col => col.IdColeccion == ped.IdColeccion).Nombre,
                        TotalUnidades = ped.TotalUnidades,
                        TotalXPedido = ped.TotalPedido,
                        SubTotalXPedido = ped.Subtotal,
                        Impuesto = ped.TotalImpuesto,
                        ClienteContadoId = ped.ClienteContadoId,
                        ModoVenta = ped.ModoVenta,
                        Flete = ped.Flete,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = ped.Clientes.CodigoCliente,
                            Nombre = ped.Clientes.Nombre,
                            Direccion = ped.Clientes.Direccion,
                            Moneda = ped.Clientes.IdMoneda,
                            EmpresaId = ped.Clientes.EmpresaId
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
                        Usuario = context.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Nombre,
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
                        }
                    }).ToList();
                        ListaPedidos.AddRange(pedidos);
                    }
                    foreach (var pedido in ListaPedidos)
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
                    return Ok(ListaPedidos);
                }
                }catch(Exception e)
                {
                    return BadRequest(e.ToString());
                }
        }

        [HttpGet]
        [Route("~/api/PedidosXCliente/Pendientes/{asesor}/{FechaInicio}/{FechaFin}")]
        public IHttpActionResult GetPendientes(string Asesor, DateTime FechaInicio, DateTime FechaFin)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }

                    List<PedidosXClienteViewModel> pedidos = context.PedidosxCliente.Where(p => p.CodigoAsesor == Asesor && p.Fecha >= FechaInicio && p.Fecha < FechaFin && p.Sincronizado==false).OrderByDescending(ped => ped.PedidoId).Select(ped => new PedidosXClienteViewModel
                    {
                        PedidoId = ped.PedidoId,
                        NumeroPedido = ped.NumeroPedido,
                        Sincronizado = ped.Sincronizado,
                        Procesando = ped.Procesando.Value,
                        ErrorAx = ped.ErrorAx,
                        NombreColeccion = context.Colecciones.FirstOrDefault(col => col.IdColeccion == ped.IdColeccion).Nombre,
                        TotalUnidades = ped.TotalUnidades,
                        TotalXPedido = ped.TotalPedido,
                        SubTotalXPedido = ped.Subtotal,
                        Impuesto = ped.TotalImpuesto,
                        ClienteContadoId = ped.ClienteContadoId,
                        ModoVenta = ped.ModoVenta,
                        Flete = ped.Flete,
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
                        Usuario = context.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Nombre,
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
                        }
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
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PedidoDetalle/{CodigoPedido}")]
        public IHttpActionResult GetPedidoDetalle(string CodigoPedido)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    List<PedidosXClienteViewModel> pedidos = context.PedidosxCliente.Where(p => p.PedidoId == CodigoPedido).Select(ped => new PedidosXClienteViewModel
                    {
                        gruposXDetPed = ped.PedidosDetalle.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
                            .Select(gruposXDetPed => new GruposTallaXDetPed
                            {
                                GrupoTalla = gruposXDetPed.Key,
                                ListaTalla = gruposXDetPed.GroupBy(pedDet => pedDet.CodigoTalla).Select(pedDet => pedDet.Key).SelectMany(pedDet => context.TallasXGrupo.Where(txp => txp.CodigoTalla == pedDet && txp.CodigoGrupoTalla == gruposXDetPed.Key)).Select(txp => new TallaViewModel
                                {
                                    GrupoTallaId = txp.CodigoGrupoTalla,
                                    Talla = txp.CodigoTalla,
                                    Orden = txp.Orden ?? 0,
                                    Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                    {
                                        IdDistribucion = dis.IdDistribucion,
                                        IdTallaxGrupo = dis.IdTallaxGrupo,
                                        NombreDistribucion = dis.NombreDistribucion,
                                        NombreTalla = dis.NombreTalla,
                                        Cantidad = dis.Cantidad,
                                    }).ToList()
                                }).OrderBy(txp => txp.Orden).ToList(),
                                prodsXDetPed = gruposXDetPed.GroupBy(pedDet => pedDet.IdProducto)
                            .Select(pedDet => new ProductosXDetPed
                            {
                                IdProducto = pedDet.Key,
                                CodigoProducto = pedDet.FirstOrDefault().ProductosxColeccion.CodigoProducto,
                                NombreProducto = pedDet.FirstOrDefault().ProductosxColeccion.NombreProducto,
                                Imagen = pedDet.FirstOrDefault().ProductosxColeccion.FotografiasXProducto.FirstOrDefault().FotografiaProducto,
                                CantidadXProducto = pedDet.Sum(cant => cant.Cantidad),
                                TotalXProducto = pedDet.Sum(cant => cant.MontoLinea),
                                coloresXProdXDetPed = pedDet.GroupBy(colXprod => colXprod.CodigoColor).Where(colXprod => colXprod.Sum(det => det.Cantidad) > 0).Select(colXprod =>
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
                                                 Talla = detPed.CodigoTalla,
                                                 TallaObject = context.TallasXGrupo.Where(txp => txp.CodigoGrupoTalla == detPed.ProductosxColeccion.CodigoGrupoTalla && txp.CodigoTalla == detPed.CodigoTalla)/*.Where(txp => false || (ped.Colecciones.ColeccionTipo == "F") || gruposXDetPed.Any(pxc => pxc.ProductosxColeccion.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) > 0))*/.Select(txp => new TallaViewModel
                                                 {
                                                     GrupoTallaId = txp.CodigoGrupoTalla,
                                                     Talla = txp.CodigoTalla,
                                                     Orden = txp.Orden ?? 0,
                                                     Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                                     {
                                                         IdDistribucion = dis.IdDistribucion,
                                                         IdTallaxGrupo = dis.IdTallaxGrupo,
                                                         NombreDistribucion = dis.NombreDistribucion,
                                                         NombreTalla = dis.NombreTalla,
                                                         Cantidad = dis.Cantidad,
                                                     }).ToList()
                                                 }).FirstOrDefault()
                                             }).ToList()

                                         }).ToList()
                            }).ToList()
                            }).ToList()
                    }).ToList();
                    return Ok(pedidos[0].gruposXDetPed);

                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

        }

        [HttpPost]
        [Route("~/api/PedidosXCliente/sincronizar/{pedido}")]
        public async Task<IHttpActionResult> PostPedidoPendiente(string pedido)
        {
            try
            {
                var Pedido = new PedidoCRMApiModel();
                using (var ctx = new AVentasEntities())
                {
                    var pedidoDB = ctx.PedidosxCliente.FirstOrDefault(x => x.PedidoId == pedido && x.Procesando==false);

                    if (pedidoDB == null)
                    {
                        return BadRequest($"El pedido {pedido} se encuentra ya en proceso de sincronizacion.");
                    }

                    Pedido = new PedidoCRMApiModel
                    {
                        COMPANY = pedidoDB.Empresa.EmpresaId,
                        CUSTOMER_ACCOUNT = pedidoDB.Clientes.CodigoCliente,
                        DATE_CONFIRMED_RECEIPT = pedidoDB.FechaEntrega.Value.ToString("dd/MM/yyyy"),
                        DELIVERY_ADDRESS = string.Empty,
                        DELIVERY_MODE = "",
                        DISC_GROUP = "",
                        ID_SALES_AGREEMENT = pedidoDB.AcuerdoVenta,
                        LINE = pedidoDB.MaestroLinea.IdLinea,
                        OBSERVATIONS = (pedidoDB.Observacion == null) ? "" : pedidoDB.Observacion,
                        PACKAGE = pedidoDB.Colecciones.CodigoColeccion,
                        PACKAGE_TYPE = pedidoDB.Colecciones.TiposdeColeccion.ColeccionTipo,
                        PedidoJsonItems = new List<PedidoJsonItems>(),
                        REFERENCE = pedidoDB.PedidoId,
                        SALES_MANAGER = pedidoDB.Asesores.Usuario,
                        SALES_ORDER_TYPE = (pedidoDB.Colecciones.TiposdeColeccion.ColeccionTipo == "B") ? "SINLOTE" : "LOTE-CONFC",
                        USER = pedidoDB.Asesores.Usuario,
                        INCLUDE_TAX = "0"


                    };
                    if (pedidoDB.ClienteContadoId != null)
                    {
                        var contado = ctx.ClienteContado.Where(c => c.id == pedidoDB.ClienteContadoId).FirstOrDefault();
                        Pedido.SALES_NAME = contado.Nombre;
                        Pedido.FISCAL_DOCUMENT = contado.RTN;
                        Pedido.DELIVERY_ADDRESS = contado.Direccion;
                        Pedido.PHONE = contado.Telefono;
                    }
                    else
                    {
                        Pedido.SALES_NAME = "";
                        Pedido.FISCAL_DOCUMENT = "";
                        Pedido.DELIVERY_ADDRESS = "";
                        Pedido.PHONE = pedidoDB.Clientes.Telefono;
                    }


                    foreach (var detalle in ctx.PedidosDetalle.Where(det => det.PedidosxCliente.PedidoId == pedidoDB.PedidoId))
                    {
                        Pedido.PedidoJsonItems.Add(
                         new PedidoJsonItems
                         {
                             COLOR = detalle.CodigoColor,
                             DELIVERY_ADDRESS = "",
                             DISC_PERCENTAGE = "0.00",
                             ITEM_CODE = detalle.ProductosxColeccion.CodigoProducto,
                             LOT_NUMBER = pedidoDB.Colecciones.CodigoColeccion,
                             QUANTITY = Convert.ToString(detalle.Cantidad),
                             REFERENCE = detalle.PedidosxCliente.PedidoId,
                             SIZE = detalle.CodigoTalla,
                             UNIT = "Und",
                             UNIT_PRICE = Convert.ToString(detalle.PrecioUnitario)
                         });
                    }

                    if (EnLinea(Pedido.COMPANY, Pedido.SALES_MANAGER))
                    {
                        string respuesta = string.Empty;
                        bool error = false;

                        pedidoDB.Procesando = true;
                        ctx.SaveChanges();

                        var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}pedidos/upload");
                        client.Timeout = 480 * (1000);
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        request.AddJsonBody(Pedido);
                        var response = client.Execute<List<string>>(request);

                        if (response.IsSuccessful)
                        {
                            if (response.Content.Substring(1, 7).ToUpper() == "SUCCESS")
                            {
                                var pedidoAX = response.Content.Substring(9, 11);
                                pedidoDB.NumeroPedido = pedidoAX;
                                pedidoDB.Sincronizado = true;
                                respuesta = $"Pedido {Pedido.REFERENCE} sincronizado exitosamente con AX.";
                            }
                        }
                        else
                        {
                            if (response.Data != null)
                            {
                                var excepcion = response.Data.FirstOrDefault();
                                var resp = JsonConvert.DeserializeObject<ApiException>(excepcion);
                                respuesta = $"Pedido {Pedido.REFERENCE} Error: {resp.Message}";
                                pedidoDB.Sincronizado = false;
                                pedidoDB.ErrorAx = resp.Message;
                                error = true;
                            }
                        }
                        pedidoDB.Procesando = false;
                        await ctx.SaveChangesAsync();

                        if (error)
                        {
                            return BadRequest(respuesta);
                        }

                        return Ok(respuesta);
                    }
                    else
                    {
                        pedidoDB.Procesando = false;
                        ctx.SaveChanges();

                        return BadRequest("Servidor de AX no disponible");
                    }
                }
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("~/api/PedidosXCliente/postax")]
        public async Task<IHttpActionResult> PostAx([FromBody] PedidoCRMApiModel pedido)
        {
            try
            {
                if (EnLinea(pedido.COMPANY, pedido.SALES_MANAGER))
                {
                    string respuesta = string.Empty;
                    bool error = false;
                    using (var ctx = new AVentasEntities())
                    {
                        var pedidoDB = ctx.PedidosxCliente.FirstOrDefault(x => x.PedidoId == pedido.REFERENCE);
                        pedidoDB.Procesando = true;
                        ctx.SaveChanges();

                        var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}pedidos/upload");
                        client.Timeout = 480 * (1000);
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        request.AddJsonBody(pedido);
                        var response = client.Execute<List<string>>(request);

                        if (response.IsSuccessful)
                        {
                            if (response.Content.Substring(1, 7).ToUpper() == "SUCCESS")
                            {
                                var pedidoAX = response.Content.Substring(9, 11);
                                pedidoDB.NumeroPedido = pedidoAX;
                                pedidoDB.Sincronizado = true;
                                respuesta = $"Pedido {pedido.REFERENCE} sincronizado exitosamente con AX.";
                            }
                        }
                        else
                        {
                            if (response.Data != null)
                            {
                                var excepcion = response.Data.FirstOrDefault();
                                var resp = JsonConvert.DeserializeObject<ApiException>(excepcion);
                                respuesta = $"Pedido {pedido.REFERENCE} Error: {resp.Message}";
                                pedidoDB.Sincronizado = false;
                                pedidoDB.ErrorAx = resp.Message;
                                error = true;
                            }
                        }
                        pedidoDB.Procesando = false;
                        await ctx.SaveChangesAsync();
                    }

                    if (error)
                    {
                        return BadRequest(respuesta);
                    }

                    return Ok(respuesta);
                }
                else
                {
                    using (var ctx = new AVentasEntities())
                    {
                        var pedidoDB = await ctx.PedidosxCliente.FirstOrDefaultAsync(x => x.PedidoId == pedido.REFERENCE);
                        pedidoDB.Procesando = false;
                        await ctx.SaveChangesAsync();
                    }
                    return BadRequest("Servidor de AX no disponible");
                }
            }catch(Exception e)
            {
                string msj = e.Message;
                try
                {
                    using (var ctx = new AVentasEntities())
                    {
                        var pedidoDB = await ctx.PedidosxCliente.FirstOrDefaultAsync(x => x.PedidoId == pedido.REFERENCE);
                        pedidoDB.Procesando = false;
                        await ctx.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    msj += "\n" + ex.Message;
                }
                return BadRequest(msj);
            }
        }


        private async void ReducirStock(PedidosxCliente pedido)
        {
            using (var ctx = new AVentasEntities())
            {
                var lineasPedido = pedido.PedidosDetalle;
                foreach (var linea in lineasPedido)
                {
                    var fisico = await ctx.FisicoDisponible.FirstOrDefaultAsync(x => x.CodigoColor == linea.CodigoColor && x.CodigoTalla == linea.CodigoTalla && x.IdProducto == linea.IdProducto);
                    fisico.Disponible = fisico.Disponible - linea.Cantidad;
                    await ctx.SaveChangesAsync();
                }
            }
        }



    }
}
