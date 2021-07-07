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
using System.Web.Script.Serialization;
using System.IO;
using AventasApi.Utils;
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
        private SyncAcuerdosVentas syncAcuerdosVentas;
        public PedidosXClienteController()
        {
            _authenticationAppService = new AuthenticationAppService();
            syncAcuerdosVentas = new SyncAcuerdosVentas();
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
                PedidosxCliente found = null;
                try
                {
                    var json = new JavaScriptSerializer().Serialize(Pedido);

                    EscribirEnArchivo($"Pedido At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var minutosConf = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotante");
                    int minutosValue = 2;

                    if(minutosConf != null)
                    { 
                        try
                        {
                            int.TryParse(minutosConf.Valor, out minutosValue);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    var fechaDesde = DateTime.Now.AddMinutes(Convert.ToDouble(minutosValue * -1));
                    var totalUnidades = Pedido.DetallePedido.Sum(x => decimal.Parse(x.Cantidad));
                    var totalPedido = Pedido.subtotal + decimal.Parse(Pedido.Impuesto.ToString()) + Pedido.Flete;

                    found = ctx.PedidosxCliente.FirstOrDefault(x => (x.Fecha >= fechaDesde  && x.Fecha <= DateTime.Now)
                                                                && x.CodigoCliente == Pedido.CodigoCliente
                                                                && x.Colecciones.CodigoColeccion == Pedido.CodigoColeccion
                                                                && x.TotalPedido == totalPedido
                                                                && x.TotalUnidades == totalUnidades
                                                                && x.CodigoAsesor == user.UserAccount);

                    if (found == null)
                    {
                        found = ctx.PedidosxCliente.FirstOrDefault(x => x.PedidoId == Pedido.NumeroReferencia);
                    }
                }

                string numeroReferencia = Pedido.NumeroReferencia;
                //int numeroCorelativo = 0;
                //var cache = false;
                

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

                if (found == null)
                {
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
                        RequiereEntrega = Pedido.RequiereEntrega,
                        BodegaEspecifica=Pedido.BodegaEspecifica,
                        Sitio=Pedido.Sitio,
                        Almacen=Pedido.Almacen,
                        Ubicacion=Pedido.Ubicacion
                    };

                    foreach (var detalle in Pedido.DetallePedido)
                    {
                        int cantidad = 0;
                        int.TryParse(detalle.Cantidad, out cantidad);
                        if (cantidad > 0)
                        {
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
                    
                    bool guardadoExito = AsyncSqlInsert.IngresarPedido(PedidoBDAGuardar, Pedido.Firma,Pedido.EmpresaUsuario);
                    if (guardadoExito)
                    {
                        using (AVentasEntities context = new AVentasEntities())
                        {
                            resultado = context.PResumenCredito().FirstOrDefault(x => x.codigocliente == cliente.CodigoCliente && x.Tipo == "Ordinario");
                        }

                        if (PedidoBDAGuardar.TotalPedido < resultado.Disponible)
                        {
                            if (cliente.FacturacionEntrega.ToUpper() == "NO" || cliente.FacturacionEntrega.ToUpper() == "NUNCA")
                            {
                                ReducirStock(PedidoBDAGuardar);
                            }
                        }

                        return Ok(new { correlativo = numeroReferencia, mensaje = "El pedido ha sido registrado con exito." });
                    }
                    else
                    {
                        PedidosxClienteFlotante PedidoFlotante = new PedidosxClienteFlotante
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
                            PedidosDetalleFlotante = new List<PedidosDetalleFlotante>(),
                            Subtotal = 0,
                            Latitude = (Pedido.location != null) ? Pedido.location.latitude : null,
                            Longitude = (Pedido.location != null) ? Pedido.location.longitude : null,
                            IdLinea = Pedido.Linea,
                            ClienteContadoId = Pedido.ClienteContadoId,
                            ModoVenta = Pedido.ModoVenta,
                            Flete = Pedido.Flete,
                            RequiereEntrega = Pedido.RequiereEntrega,
                            ESTADO = 0,
                            BodegaEspecifica = Pedido.BodegaEspecifica,
                            Sitio = Pedido.Sitio,
                            Almacen = Pedido.Almacen,
                            Ubicacion=Pedido.Ubicacion
                        };

                        //if (numeroReferencia == "")
                        //{
                        //    cache = true;
                        //    numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                        //    string inicialesAsesor = asesor.InicialesNombre;
                        //    numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";
                        //}

                        foreach (var detalle in Pedido.DetallePedido)
                        {
                            int cantidad = 0;
                            int.TryParse(detalle.Cantidad, out cantidad);
                            if (cantidad > 0)
                            {
                                PedidoFlotante.TotalUnidades += cantidad;
                                decimal precioUnitario = 0;
                                decimal.TryParse(detalle.PrecioUnitario, out precioUnitario);
                                PedidoFlotante.Subtotal += (precioUnitario * cantidad);

                                PedidoFlotante.PedidosDetalleFlotante.Add(new PedidosDetalleFlotante
                                {
                                    PedidoId = numeroReferencia,
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

                        PedidoFlotante.TotalImpuesto = Pedido.Impuesto;
                        PedidoFlotante.TotalPedido = (PedidoFlotante.Subtotal.Value + decimal.Parse(Pedido.Impuesto.ToString())) + Pedido.Flete;
                        PedidoFlotante.PedidoId = numeroReferencia;
                        PedidoFlotante.NumeroPedido = "";
                        PedidoFlotante.Sincronizado = false;
                        PedidoFlotante.Procesando = false;


                        AsyncSqlInsert.IngresarPedidoFlotante(PedidoFlotante, Pedido.Firma);
                    }

                    return Ok(new { correlativo = numeroReferencia, mensaje = "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de pedidos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento comercial para que procedan a revisar y gestionar su pedido para que sea válido." });

                    //s_ = PostPedidoPendiente(numeroReferencia);
                }
                else
                {
                    PedidosxClienteFlotante PedidoBDAGuardar = new PedidosxClienteFlotante
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
                        PedidosDetalleFlotante = new List<PedidosDetalleFlotante>(),
                        Subtotal = 0,
                        Latitude = (Pedido.location != null) ? Pedido.location.latitude : null,
                        Longitude = (Pedido.location != null) ? Pedido.location.longitude : null,
                        IdLinea = Pedido.Linea,
                        ClienteContadoId = Pedido.ClienteContadoId,
                        ModoVenta = Pedido.ModoVenta,
                        Flete = Pedido.Flete,
                        RequiereEntrega = Pedido.RequiereEntrega,
                        ESTADO = 0,
                        BodegaEspecifica = Pedido.BodegaEspecifica,
                        Sitio = Pedido.Sitio,
                        Almacen = Pedido.Almacen,
                        Ubicacion=Pedido.Ubicacion
                    };

                    //if (numeroReferencia == "")
                    //{
                    //    cache = true;
                    //    numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                    //    string inicialesAsesor = asesor.InicialesNombre;
                    //    numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";
                    //}

                    foreach (var detalle in Pedido.DetallePedido)
                    {
                        int cantidad = 0;
                        int.TryParse(detalle.Cantidad, out cantidad);
                        if (cantidad > 0)
                        {
                            PedidoBDAGuardar.TotalUnidades += cantidad;
                            decimal precioUnitario = 0;
                            decimal.TryParse(detalle.PrecioUnitario, out precioUnitario);
                            PedidoBDAGuardar.Subtotal += (precioUnitario * cantidad);

                            PedidoBDAGuardar.PedidosDetalleFlotante.Add(new PedidosDetalleFlotante
                            {
                                PedidoId = numeroReferencia,
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


                    AsyncSqlInsert.IngresarPedidoFlotante(PedidoBDAGuardar, Pedido.Firma);
                }

                return Ok(new { correlativo= numeroReferencia,mensaje= "El documento creado ha sido enviado al flujo de flotantes por validaciones de sistema. Verifíque en el listado de pedidos si este se encuentra ya creado correctamente. De lo contrario, contacte con el departamento comercial para que procedan a revisar y gestionar su pedido para que sea válido." });
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PedidosXCliente/correlativo/{empresa}")]
        public async Task <IHttpActionResult> GetCorrelativo(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId==empresa);
                    int numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    string numeroReferencia = $"{inicialesAsesor}-1{numeroCorelativo.ToString("D5")}";

                    /*if (aumentar == 1)
                    {
                        var toUpdate = ctx.Asesores.FirstOrDefault(x => x.CodigoAsesor == asesor.CodigoAsesor);
                        if (toUpdate != null)
                        {
                            toUpdate.CorrelativoPedidos = numeroCorelativo + 1;
                            ctx.SaveChanges();
                        }
                    }*/


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
                    foreach (var asesor in asesoresHabilitados.Distinct().ToList())
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
                        BodegaEspecifica=ped.BodegaEspecifica,
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
                        BodegaEspecifica=ped.BodegaEspecifica,
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
                                        Orden = dis.Orden
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
                                                         Orden = dis.Orden
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
                        SALES_MANAGER = pedidoDB.CodigoAsesor,
                        SALES_ORDER_TYPE = (pedidoDB.Colecciones.TiposdeColeccion.ColeccionTipo == "B") ? "SINLOTE" : "LOTE-CONFC",
                        USER = pedidoDB.CodigoAsesor,
                        INCLUDE_TAX = "0",
                        ESPEC_INV = pedidoDB.BodegaEspecifica == null ? "0" : (pedidoDB.BodegaEspecifica.Value ? "1" : "0"),
                        LOCATION = pedidoDB.Almacen,
                        SITE = pedidoDB.Sitio
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
                                syncAcuerdosVentas.SyncAcuerdoVenta(Pedido.COMPANY, Pedido.CUSTOMER_ACCOUNT, Pedido.USER);
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
                            syncAcuerdosVentas.SyncAcuerdoVenta(pedido.COMPANY, pedido.CUSTOMER_ACCOUNT, pedido.USER);
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

        [HttpGet]
        [Route("~/api/PedidosXCliente/Flotantes/{FechaInicio}/{FechaFin}/{estado}/{asesor}")]
        public async Task<IHttpActionResult> GetFlotantes(DateTime FechaInicio, DateTime FechaFin, int estado,string asesor)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);                    

                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }
                    
                    List<PedidosXClienteViewModel> pedidosFlotantes = ctx.PedidosxClienteFlotante.Where(x => x.ESTADO == estado && x.Fecha >= FechaInicio && x.Fecha < FechaFin && x.CodigoAsesor==asesor).OrderByDescending(x => x.PedidoId).Select(ped => new PedidosXClienteViewModel
                    {
                        Id = ped.Id,
                        Asesor = ped.CodigoAsesor,
                        PedidoId = ped.PedidoId,
                        NumeroPedido = ped.NumeroPedido,
                        Sincronizado = ped.Sincronizado,
                        BodegaEspecifica = ped.BodegaEspecifica,
                        NombreColeccion = ctx.Colecciones.FirstOrDefault(col => col.IdColeccion == ped.IdColeccion).Nombre,
                        TotalUnidades = ped.TotalUnidades,
                        TotalXPedido = ped.TotalPedido,
                        SubTotalXPedido = ped.Subtotal,
                        Impuesto = ped.TotalImpuesto,
                        ClienteContadoId = ped.ClienteContadoId,
                        ModoVenta = ped.ModoVenta,
                        Flete = ped.Flete,
                        Estado = ped.ESTADO,
                        PedidoGenerado = ped.PedidoIdGenerado == null ? "No Disponible" : ped.PedidoIdGenerado,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = ped.Clientes.CodigoCliente,
                            Nombre = ped.Clientes.Nombre,
                            Direccion = ped.Clientes.Direccion,
                            Moneda = ped.Clientes.IdMoneda,
                            EmpresaId = ped.Clientes.EmpresaId
                        },
                        Linea = ctx.MaestroLinea.Select(ml => new LineaViewModel
                        {
                            IdLinea = ml.IdLinea,
                            Linea = ml.Linea,
                        }).FirstOrDefault(ml => ml.IdLinea == ped.IdLinea),
                        TipoPedido = ctx.TiposdePedido.Select(tp => new TipoPedidoViewModel
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
                        Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Nombre,
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

                    foreach (var pedido in pedidosFlotantes)
                    {
                        string imagenB64 = "";

                        var firma = ctx.FirmasxPedido.FirstOrDefault(fir => pedido.PedidoId == fir.PedidoId);
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

                    return Ok(pedidosFlotantes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PedidosXCliente/Flotantes/{FechaInicio}/{FechaFin}/{estado}")]
        public async Task<IHttpActionResult> GetFlotantesAsesores(DateTime FechaInicio, DateTime FechaFin, int estado)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        asesoresHabilitados = await ctx.Asesores.Where(x =>x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    if (FechaInicio == DateTime.Parse("1900-01-01") || FechaFin == DateTime.Parse("1900-01-01"))
                    {
                        FechaInicio = DateTime.Today.AddDays(-30);
                        FechaFin = DateTime.Today.AddDays(1);
                    }
                    else
                    {
                        FechaFin = FechaFin.AddDays(1);
                    }

                    List<PedidosXClienteViewModel> ListaPedidosFlotantes = new List<PedidosXClienteViewModel>();

                    foreach (var asesor in asesoresHabilitados.Distinct().ToList())
                    {
                        List<PedidosXClienteViewModel> pedidosFlotantes = ctx.PedidosxClienteFlotante.Where(x => x.ESTADO == estado && x.Fecha >= FechaInicio && x.Fecha < FechaFin && x.CodigoAsesor == asesor).OrderByDescending(x => x.PedidoId).Select(ped => new PedidosXClienteViewModel
                        {
                            Id = ped.Id,
                            Asesor = ped.CodigoAsesor,
                            PedidoId = ped.PedidoId,
                            NumeroPedido = ped.NumeroPedido,
                            Sincronizado = ped.Sincronizado,
                            BodegaEspecifica = ped.BodegaEspecifica,
                            NombreColeccion = ctx.Colecciones.FirstOrDefault(col => col.IdColeccion == ped.IdColeccion).Nombre,
                            TotalUnidades = ped.TotalUnidades,
                            TotalXPedido = ped.TotalPedido,
                            SubTotalXPedido = ped.Subtotal,
                            Impuesto = ped.TotalImpuesto,
                            ClienteContadoId = ped.ClienteContadoId,
                            ModoVenta = ped.ModoVenta,
                            Flete = ped.Flete,
                            Estado = ped.ESTADO,
                            PedidoGenerado = ped.PedidoIdGenerado == null ? "No Disponible" : ped.PedidoIdGenerado,
                            Cliente = new ClienteViewModel
                            {
                                Codigo = ped.Clientes.CodigoCliente,
                                Nombre = ped.Clientes.Nombre,
                                Direccion = ped.Clientes.Direccion,
                                Moneda = ped.Clientes.IdMoneda,
                                EmpresaId = ped.Clientes.EmpresaId
                            },
                            Linea = ctx.MaestroLinea.Select(ml => new LineaViewModel
                            {
                                IdLinea = ml.IdLinea,
                                Linea = ml.Linea,
                            }).FirstOrDefault(ml => ml.IdLinea == ped.IdLinea),
                            TipoPedido = ctx.TiposdePedido.Select(tp => new TipoPedidoViewModel
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
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == ped.CodigoAsesor).Nombre,
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

                        foreach (var pedido in pedidosFlotantes)
                        {
                            string imagenB64 = "";

                            var firma = ctx.FirmasxPedido.FirstOrDefault(fir => pedido.PedidoId == fir.PedidoId);
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

                        ListaPedidosFlotantes.AddRange(pedidosFlotantes);
                    }

                    return Ok(ListaPedidosFlotantes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/PedidosXCliente/Flotantes/cancelar/{id}")]
        public async Task<IHttpActionResult> CancelarPedidoFlotante(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    PedidosxClienteFlotante pedido = await ctx.PedidosxClienteFlotante.FindAsync(id);

                    if (pedido == null)
                    {
                        return BadRequest("El pedido no existe.");
                    }

                    pedido.ESTADO = 2;
                    pedido.EditedBy = user.UserAccount;
                    pedido.EditedDate = DateTime.Now;
                    int res = await ctx.SaveChangesAsync();
                    return Ok(res);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/PedidosXCliente/Flotantes/sincronizar/{id}")]
        public async Task<IHttpActionResult> SincronizarPedidoFlotante(int id)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    PedidosxClienteFlotante pedido = await ctx.PedidosxClienteFlotante.FindAsync(id);

                    if (pedido == null)
                    {
                        return BadRequest("El pedido no existe.");
                    }

                    var asesor = await ctx.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor == pedido.CodigoAsesor && x.CorrelativoPedidos!=null);
                    int numeroCorelativo = asesor.CorrelativoPedidos ?? 0;
                    string numeroReferencia = $"{asesor.InicialesNombre}-1{numeroCorelativo.ToString("D5")}";

                    PedidosxCliente PedidoBDAGuardar = new PedidosxCliente
                    {
                        IdTipoPedido = pedido.IdTipoPedido,
                        IdColeccion = pedido.IdColeccion,
                        CodigoCliente = pedido.CodigoCliente,
                        AcuerdoVenta = pedido.AcuerdoVenta,
                        EmpresaId = pedido.EmpresaId,
                        Fecha = pedido.Fecha,
                        FechaEntrega = pedido.FechaEntrega,
                        CodigoAsesor = pedido.CodigoAsesor,
                        Observacion = pedido.Observacion,
                        TotalUnidades = pedido.TotalUnidades,
                        PedidosDetalle = new List<PedidosDetalle>(),
                        Subtotal = pedido.Subtotal,
                        Latitude = pedido.Latitude,
                        Longitude = pedido.Longitude,
                        IdLinea = pedido.IdLinea,
                        ClienteContadoId = pedido.ClienteContadoId,
                        ModoVenta = pedido.ModoVenta,
                        Flete = pedido.Flete,
                        RequiereEntrega = pedido.RequiereEntrega,
                        TotalImpuesto = pedido.TotalImpuesto,
                        TotalPedido = pedido.TotalPedido,
                        Sincronizado = false,
                        Procesando = false,
                        PedidoId = numeroReferencia,
                        NumeroPedido = "",
                        BodegaEspecifica=pedido.BodegaEspecifica,
                        Sitio=pedido.Sitio,
                        Almacen=pedido.Almacen
                    };

                    foreach (var detalle in pedido.PedidosDetalleFlotante)
                    {
                        PedidoBDAGuardar.PedidosDetalle.Add(new PedidosDetalle
                        {
                            IdProducto = detalle.IdProducto,
                            CodigoColor = detalle.CodigoColor,
                            CodigoTalla = detalle.CodigoTalla,
                            Cantidad = detalle.Cantidad,
                            MontoLinea = detalle.MontoLinea,
                            Fecha = detalle.Fecha,
                            CodigoAsesor = asesor.CodigoAsesor,
                            PrecioUnitario = detalle.PrecioUnitario
                        });
                    }

                    ctx.PedidosxCliente.Add(PedidoBDAGuardar);
                    int rowAffected = await ctx.SaveChangesAsync();
                    if (rowAffected > 0)
                    {
                        asesor.CorrelativoPedidos = asesor.CorrelativoPedidos + 1;
                        await ctx.SaveChangesAsync();
                    }

                    var firma = await ctx.FirmasxPedido.FirstOrDefaultAsync(fir => pedido.PedidoId == fir.PedidoId);
                    if (firma != null)
                    {
                        firma.PedidoId = numeroReferencia;
                    }
                    pedido.PedidoIdGenerado = numeroReferencia;
                    pedido.ESTADO = 1;
                    pedido.EditedBy = user.UserAccount;
                    pedido.EditedDate = DateTime.Now;
                    await ctx.SaveChangesAsync();

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/PedidoDetalle/flotante/{CodigoPedido}")]
        public IHttpActionResult GetPedidoDetalle(int CodigoPedido)
        {
            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    List<PedidosXClienteViewModel> pedidos = context.PedidosxClienteFlotante.Where(p => p.Id == CodigoPedido).Select(ped => new PedidosXClienteViewModel
                    {
                        gruposXDetPed = ped.PedidosDetalleFlotante.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
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
                                        Orden = dis.Orden
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
                                                         Orden = dis.Orden
                                                     }).ToList()
                                                 }).FirstOrDefault()
                                             }).ToList()

                                         }).ToList()
                            }).ToList()
                            }).ToList()
                    }).ToList();
                    return Ok(pedidos[0].gruposXDetPed);

                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

        }


        private async void ReducirStock(PedidosxCliente pedido)
        {
            using (var ctx = new AVentasEntities())
            {
                var lineasPedido = pedido.PedidosDetalle;
                foreach (var linea in lineasPedido)
                {
                    var fisico = await ctx.FisicoDisponible.FirstOrDefaultAsync(x => x.CodigoColor == linea.CodigoColor && x.CodigoTalla == linea.CodigoTalla && x.IdProducto == linea.IdProducto && x.Sitio==pedido.Sitio && x.Almacen==x.Almacen);
                    fisico.Disponible = fisico.Disponible - linea.Cantidad;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public void EscribirEnArchivo(string Message)
        {
            try
            {
                #region Creacion Carpeta
                string path = @"C:\AVentasAPIPedidos";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                #endregion Creacion Carpeta

                #region Creacion Archivo
                string filepath = path + "\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
                #endregion Creacion Archivo
            }
            catch (Exception ex)
            {

            }
        }

    }
}
