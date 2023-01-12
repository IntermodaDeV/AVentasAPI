using AventasApi.Models.ViewModels;
using AventasApi.Services.AsyncJobs;
using AventasApi.Services.Authentication;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using ExternalApiData.ApiModels;
using RestSharp;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using AventasApi.Models;
using AventasApi.Utils;
using System.IO;
using System.Web.Script.Serialization;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/devolucion")]
    public class DevolucionController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public DevolucionController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        private bool EnLinea(string empresa, string asesor)
        {
            var client = new RestClient(Enviroment.CRMWebServiceURLApi);
            client.Authenticator = new RestSharp.Authenticators.NtlmAuthenticator();
            var request = new RestRequest($"asesor/{empresa}/{asesor}", Method.GET);
            client.Timeout = 6000;
            IRestResponse<List<AsesorApiModel>> respuesta = client.Execute<List<AsesorApiModel>>(request);

            return respuesta.IsSuccessful;
        }

        [HttpGet]
        [Route("{correlativo}")]
        public async Task<IHttpActionResult> ObtenerDevolucion(string correlativo)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<DevolucionesViewModel> devolucion = await ctx.Devolucion.Where(x=>x.NumDevolucion==correlativo).Select(x=> new DevolucionesViewModel
                    {
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.NumeroRMA,
                        PedidoDevolucion = x.PedidoDevolucion,
                        CodigoCliente = x.CodigoCliente,
                        NombreCliente = x.Clientes.Nombre,
                        motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        TotalUnidades = x.TotalUnidades,
                        Estado = x.Estado,
                        FechaCreacion = x.FechaCrea.Value,
                        SubTotal = x.Subtotal,
                        Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = x.Clientes.CodigoCliente,
                            Nombre = x.Clientes.Nombre,
                            Direccion = x.Clientes.Direccion,
                            Moneda = x.Clientes.IdMoneda,
                            EmpresaId = x.Clientes.EmpresaId
                        }
                    }).ToListAsync();

                  return Ok(devolucion.First());
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("listadoDevPendienteAprobar")]
        public IHttpActionResult ObtenerDevolucionesPendientesAprobar()
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var listaDevoluciones = db.AprobacionDevoluciones.Where(x => x.IdUsuario == user.Id && x.Aprobado == false && x.Estado == true).Select(x => new 
                    {
                        IdDevAprobacion = x.IdDevAprobacion,
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.Devolucion.NumeroRMA,
                        PedidoDevolucion = x.Devolucion.PedidoDevolucion,
                        CodigoCliente = x.Devolucion.CodigoCliente,
                        NombreCliente = x.Devolucion.Clientes.Nombre,
                        Linea = x.Devolucion.IdLinea,
                        MotivoDevolucion = x.Devolucion.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        TotalUnidades = x.Devolucion.TotalUnidades,
                        Estado = x.Devolucion.Estado,
                        FacturaOrigen = x.Devolucion.FacturaOrigen,
                        PedidoOrigen = x.Devolucion.PedidoOrigen,
                        FechaCreacion = x.FechaCrea,
                        SubTotal = x.Devolucion.Subtotal,
                        Usuario = db.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.Devolucion.CodigoAsesor).Nombre,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = x.Devolucion.Clientes.CodigoCliente,
                            Nombre = x.Devolucion.Clientes.Nombre,
                            Direccion = x.Devolucion.Clientes.Direccion,
                            Moneda = x.Devolucion.Clientes.IdMoneda,
                            EmpresaId = x.Devolucion.Clientes.EmpresaId
                        }
                    }).ToList();
                    return Ok(listaDevoluciones);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("aprobarDevoluciones/{idDevAprobacion}")]
        public IHttpActionResult aprobarDevoluciones(int idDevAprobacion)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var listaDevoluciones = db.AprobacionDevoluciones.FirstOrDefault(x => x.IdDevAprobacion == idDevAprobacion);

                    if (listaDevoluciones == null)
                    {
                        return BadRequest("No existe el registro");
                    }

                    listaDevoluciones.Aprobado = true;
                    listaDevoluciones.UsuarioModifica = user.Id;
                    listaDevoluciones.FechaModifica = DateTime.Now;
                    var result = db.SaveChanges();
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
        
        
        [HttpPost]
        [Route("rechazarDevoluciones/{numDevAprobacion}/{justificado}")]
        public IHttpActionResult rechazarDevoluciones(string numDevAprobacion, string justificado)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var AprobacionDevoluciones = db.AprobacionDevoluciones.Where(x => x.NumDevolucion == numDevAprobacion).ToList() ;


                    var Devolucion = db.Devolucion.FirstOrDefault(x => x.NumDevolucion == numDevAprobacion);

                    if (AprobacionDevoluciones == null)
                    {
                        return BadRequest("No existe el registro");
                    } 
                    
                    if (Devolucion == null)
                    {
                        return BadRequest("No existe el registro");
                    }

                    AprobacionDevoluciones.ForEach(x => x.Estado = false);
                    AprobacionDevoluciones.ForEach(x => x.UsuarioModifica = user.Id);
                    AprobacionDevoluciones.ForEach(x => x.FechaModifica = DateTime.Now);

                    Devolucion.Observacion = justificado;
                    Devolucion.Estado = "No autorizado";
                    Devolucion.Sincronizado = true;
                 
                    var result = db.SaveChanges();
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("sincronizar/{devolucion}")]
        public async Task<IHttpActionResult> PostDevolucionPendiente(string devolucion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var DevolucionesPedientes = ctx.AprobacionDevoluciones.Where(x => x.Aprobado == false && x.NumDevolucion == devolucion).Select(x => x.Usuarios.nombre).Distinct().ToList();

                    if (DevolucionesPedientes.Count() > 0)
                    {
                        string nombres = string.Empty;

                        foreach (string nombre in DevolucionesPedientes)
                        {
                            if (string.IsNullOrEmpty(nombres))
                            {
                                nombres = nombre;
                            }
                            else
                            {
                                nombres = $"{nombres}, {nombre}";
                            }
                        }

                        return BadRequest($"El pedido {devolucion} se encuentra pendiente a la aprobacion de: {nombres}.");
                    }
                    var devolucionDB = await ctx.Devolucion.FirstOrDefaultAsync(x => x.NumDevolucion == devolucion && x.Procesando == false);
                    var ubicacion = await ctx.UbicacionesXAlmacen.FirstOrDefaultAsync(x => x.MaestroBodegaAlmacenes.Almacen == devolucionDB.almacen && x.MaestroBodegaAlmacenes.EmpresaId == devolucionDB.EmpresaId && x.ActivoDevolucion == true);

                    if (devolucionDB == null)
                    {
                        return BadRequest($"El pedido {devolucion} se encuentra ya en proceso de sincronizacion.");
                    }


                    var devolucionApi = new DevolucionApiModel
                    {
                        COMPANY = devolucionDB.Empresa.EmpresaId,
                        CUSTOMER_ACCOUNT = devolucionDB.Clientes.CodigoCliente,
                        SALES_MANAGER = devolucionDB.CodigoAsesor,
                        USER = devolucionDB.CodigoAsesor,
                        OBSERVATIONS = (devolucionDB.Observacion == null) ? "" : devolucionDB.Observacion,
                        REASON_CODE = devolucionDB.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        REFERENCE = devolucionDB.NumDevolucion,
                        SALES_NAME = devolucionDB.Clientes.Nombre,
                        LINE = string.IsNullOrEmpty(devolucionDB.IdLinea) ? "TPT" : devolucionDB.IdLinea,
                        LOCATION = devolucionDB.almacen,
                    };

                    foreach (var detalle in ctx.DevolucionDetalle.Where(det => det.Devolucion.NumDevolucion == devolucion))
                    {
                        devolucionApi.DevolucionDetalleJson.Add(
                         new DevolucionDetalleJson
                         {
                             COLOR = detalle.CodigoColor.Trim(),
                             ITEM_CODE = detalle.ProductosxColeccion.CodigoProducto,
                             QUANTITY = Convert.ToString(detalle.Cantidad),
                             SIZE = detalle.CodigoTalla.Trim(),
                             REFERENCE = detalle.NumDevolucion,
                             SALES_NUMBER = devolucionDB.PedidoOrigen,
                             UNIT = "Und",
                             UBICATION = ubicacion.CodigoUbicacion
                         });
                    }

                    if (EnLinea(devolucionApi.COMPANY, devolucionApi.SALES_MANAGER))
                    {
                        string respuesta = string.Empty;
                        bool error = false;

                        devolucionDB.Procesando = true;
                        await ctx.SaveChangesAsync();

                    var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                    var request = new RestRequest($"devoluciones/registrar", Method.POST);
                    request.Timeout = 480 * (2000);
                    request.AddHeader("Accept", "application/json");
                    request.AddJsonBody(devolucionApi);
                    var response = restClient.Execute<List<string>>(request);

                    if (response.IsSuccessful)
                        {
                            var content = JsonConvert.DeserializeObject<string>(response.Content); ;

                            if (content.StartsWith("Success"))
                            {
                                content = content.Remove(0, 8);
                                var probando = content.Split(',');
                                devolucionDB.NumeroRMA = probando[0].Trim();
                                devolucionDB.PedidoDevolucion = probando[1].Trim();
                                devolucionDB.Estado = "Creado";
                                devolucionDB.Sincronizado = true;
                                devolucionDB.Procesando = false;
                                respuesta = $"Devolucion {devolucionApi.REFERENCE} sincronizado exitosamente con AX.";
                            }
                        }
                        else
                        {
                            if (response.Data != null)
                            {
                                var excepcion = response.Data.FirstOrDefault();
                                var resp = JsonConvert.DeserializeObject<ApiException>(excepcion);
                                respuesta = $"Devolucion {devolucionApi.REFERENCE} Error: {resp.Message}";
                                devolucionDB.Sincronizado = false;
                                devolucionDB.ErrorAx = resp.Message;
                                error = true;
                            }
                        }

                        devolucionDB.Procesando = false;
                        await ctx.SaveChangesAsync();

                        if (error)
                        {
                            return BadRequest(respuesta);
                        }
                        return Ok(respuesta);
                    }
                    else
                    {
                        devolucionDB.Procesando = false;
                        await ctx.SaveChangesAsync();
                        return BadRequest("Servidor de AX no disponible");
                    }
                
                 }

            }
            catch (Exception e)
            {

                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("listado/{asesor}")]
        public async Task<IHttpActionResult> ObtenerlistadoDevoluciones(string asesor)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    List<DevolucionesViewModel> devoluciones = await ctx.Devolucion.Where(x => x.CodigoAsesor == asesor).Select(x => new DevolucionesViewModel
                    {
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.NumeroRMA,
                        PedidoDevolucion = x.PedidoDevolucion,
                        CodigoCliente = x.CodigoCliente,
                        NombreCliente = x.Clientes.Nombre,
                        motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        TotalUnidades = x.TotalUnidades,
                        Estado = x.Estado,
                        FechaCreacion = x.FechaCrea.Value,
                        SubTotal = x.Subtotal,
                        Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                        Observacion = x.Observacion,
                        UsuarioModifica = x.AprobacionDevoluciones.FirstOrDefault().Usuarios1.nombre,
                        EstadoBodega = x.EstadoBodega,
                        Cliente = new ClienteViewModel
                        {
                            Codigo = x.Clientes.CodigoCliente,
                            Nombre = x.Clientes.Nombre,
                            Direccion = x.Clientes.Direccion,
                            Moneda = x.Clientes.IdMoneda,
                            EmpresaId = x.Clientes.EmpresaId
                        }
                    }).ToListAsync();

                    return Ok(devoluciones);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("detalle/{correlativo}")]
        public IHttpActionResult ObtenerDetalleDevolucion(string correlativo)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<PedidosXClienteViewModel> devoluciones = ctx.Devolucion.Where(x => x.NumDevolucion == correlativo).Select(dev => new PedidosXClienteViewModel
                    {
                        gruposXDetPed = dev.DevolucionDetalle.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
                        .Select(gruposXDetPed => new GruposTallaXDetPed
                        {
                            GrupoTalla = gruposXDetPed.Key,
                            ListaTalla = gruposXDetPed.GroupBy(pedDet => pedDet.CodigoTalla).Select(pedDet => pedDet.Key).SelectMany(pedDet => ctx.TallasXGrupo.Where(txp => txp.CodigoTalla.ToUpper().Trim() == pedDet.ToUpper().Trim() && txp.CodigoGrupoTalla.ToUpper().Trim() == gruposXDetPed.Key.ToUpper().Trim())).Select(txp => new TallaViewModel
                            {
                                GrupoTallaId = txp.CodigoGrupoTalla.ToUpper(),
                                Talla = txp.CodigoTalla.ToUpper(),
                                Orden = txp.Orden ?? 0,
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
                                             NombreColor = ctx.Colores.FirstOrDefault(color => color.CodigoColor == colXprod.Key).Color,
                                             DetallesXPedido = colXprod.Select(detPed => new DetalleXPedidoViewModel
                                             {
                                                 IdRegistro = detPed.IdDevolucionDetalle,
                                                 PedidoId = detPed.NumDevolucion,
                                                 Cantidad = detPed.Cantidad,
                                                 MontoLinea = 0,
                                                 PrecioUnitario = detPed.PrecioUnitario,
                                                 Talla = detPed.CodigoTalla.ToUpper(),
                                                 TallaObject = ctx.TallasXGrupo.Where(txp => txp.CodigoGrupoTalla == detPed.ProductosxColeccion.CodigoGrupoTalla && txp.CodigoTalla == detPed.CodigoTalla).Select(txp => new TallaViewModel
                                                 {
                                                     GrupoTallaId = txp.CodigoGrupoTalla,
                                                     Talla = txp.CodigoTalla.ToUpper(),
                                                     Orden = txp.Orden ?? 0,
                                                 }).FirstOrDefault()
                                             }).ToList()

                                         }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();

                    return Ok(devoluciones[0].gruposXDetPed);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("listado/pendiente")]
        public IHttpActionResult ObtenerlistadoDevolucionesPendientes()
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var listaCitas = db.Devolucion.Where(x => x.CodigoAsesor == user.UserAccount && x.Sincronizado == false).Select(x => new DevolucionesViewModel
                    {
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.NumeroRMA,
                        PedidoDevolucion = x.PedidoDevolucion,
                        CodigoCliente = x.CodigoCliente,
                        NombreCliente = x.Clientes.Nombre,
                        motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        Estado = x.Estado,
                        ErrorAx = x.ErrorAx,
                        Linea = x.IdLinea
                    }).ToList();
                    return Ok(listaCitas);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("correlativo/{empresa}")]
        public async Task<IHttpActionResult> GetCorrelativo(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId == empresa);
                    int numeroCorelativo = asesor.CorrelativoDevolucion ?? 0;
                    string inicialesAsesor = asesor.InicialesNombre;
                    string numeroReferencia = $"{inicialesAsesor}DEV-1{numeroCorelativo.ToString("D5")}";

                    return Ok(numeroReferencia);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("completa")]
        public async Task<IHttpActionResult> PostDevolucion([FromBody]DevolucionPostModel devolucion)
        {
            try
            {
                try
                {
                    var json = new JavaScriptSerializer().Serialize(devolucion);

                    EscribirLogDevolucion($"Devolucion At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }

                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    Usuarios usuario = await ctx.Usuarios.FindAsync(user.Id);
                    Clientes cliente = await ctx.Clientes.FindAsync(devolucion.CodigoCliente);
                    var PendienteAprobacion = await ctx.MotivosDevConAprobacion.Where(x => x.IdMotivoDevolucion == devolucion.MotivoDevolucion && x.Estado == true).ToListAsync();
                    var usuariosCorreo = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == usuario.Id && x.EmpresaId == devolucion.Empresa).Select(x => x.UsuarioId).ToListAsync();
                    var correos = await ctx.Usuarios.Where(x => x.CorreoDevolucion == true && x.Correo != null && usuariosCorreo.Contains(x.Id)).Select(x => x.Correo).ToListAsync();
                    var motivoDetalle = await ctx.MotivosDevolucionDetalle.FindAsync(devolucion.MotivoDevolucionDetalle);

                    var minutosConf = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotante");
                    int minutosValue = 2;

                    if (minutosConf != null)
                    {
                        try
                        {
                            int.TryParse(minutosConf.Valor, out minutosValue);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    Devolucion found = null;
                    var fechaDesde = DateTime.Now.AddMinutes(Convert.ToDouble(minutosValue * -1));
                    var totalUnidades = devolucion.DetalleDevolucion.Sum(x => decimal.Parse(x.Cantidad.ToString()));
                    var totalPedido = devolucion.SubTotal;
                    var ubicacion = await ctx.UbicacionesXAlmacen.FirstOrDefaultAsync(x => x.MaestroBodegaAlmacenes.Almacen == devolucion.Almacen && x.MaestroBodegaAlmacenes.EmpresaId == devolucion.Empresa && x.ActivoDevolucion == true);
                    
                    found = ctx.Devolucion.FirstOrDefault(x => (x.FechaCrea >= fechaDesde && x.FechaCrea <= DateTime.Now)
                                                                && x.CodigoCliente == devolucion.CodigoCliente
                                                                && x.Subtotal == totalPedido
                                                                && x.TotalUnidades == totalUnidades
                                                                && x.CodigoAsesor == user.UserAccount);

                    if (found == null)
                    {
                        found = ctx.Devolucion.FirstOrDefault(x => x.NumDevolucion == devolucion.Correlativo);
                    }

                    if (found == null)
                    {
                        Devolucion devolucionDB = new Devolucion()
                        {
                            NumDevolucion = devolucion.Correlativo,
                            CodigoCliente = devolucion.CodigoCliente,
                            IdLinea = devolucion.Linea,
                            IdMotivoDevDetalle = devolucion.MotivoDevolucionDetalle,
                            EmpresaId = devolucion.Empresa,
                            PedidoOrigen = devolucion.PedidoOriginal,
                            FacturaOrigen = devolucion.FacturaOriginal,
                            CodigoAsesor = cliente.CodigoAsesor,
                            UsuarioCrea = user.Id,
                            FechaCrea = DateTime.Now,
                            Sincronizado = false,
                            Estado = PendienteAprobacion.Count > 0 ? "Pendiente Aprobacion" : "No Sincronizado",
                            Subtotal = devolucion.SubTotal,
                            TotalUnidades = 0,
                            almacen = devolucion.Almacen,
                            IdUbicacion = ubicacion.UbicacionId
                        };

                        foreach (DevolucionDetallePostModel detalle in devolucion.DetalleDevolucion)
                        {
                            devolucionDB.TotalUnidades += detalle.Cantidad;

                            devolucionDB.DevolucionDetalle.Add(new DevolucionDetalle()
                            {
                                NumDevolucion = devolucion.Correlativo,
                                IdProducto = detalle.IdProducto,
                                CodigoColor = detalle.CodigoColor,
                                CodigoTalla = detalle.CodigoTalla,
                                Cantidad = detalle.Cantidad,
                                PrecioUnitario = detalle.PrecioUnitario,
                                MontoLinea = detalle.Cantidad * detalle.PrecioUnitario
                            });
                        }
                        bool guardadoExito = AsyncSqlInsert.IngresarDevolucion(devolucionDB, usuario.EmpresaId);

                        if (!guardadoExito)
                        {
                            return BadRequest("No se pudo guardar la devolucion.");
                        }

                        if (PendienteAprobacion.Count > 0)
                        {
                            foreach (var x in PendienteAprobacion)
                            {
                                AprobacionDevoluciones aprobacionDevoluciones = new AprobacionDevoluciones()
                                {
                                    IdUsuario = x.IdUsuario,
                                    NumDevolucion = devolucion.Correlativo,
                                    Estado = true,
                                    UsuarioCrea = user.Id,
                                    FechaCrea = DateTime.Now
                                };
                                ctx.AprobacionDevoluciones.Add(aprobacionDevoluciones);
                                var result = await ctx.SaveChangesAsync();
                            }
                        }

                        if (!string.IsNullOrEmpty(devolucion.FacturaOriginal) || !string.IsNullOrWhiteSpace(devolucion.FacturaOriginal))
                        {
                            ReducirPendienteDevolucion(devolucionDB);
                        }


                        _ = new Email().EnviarEmail($"Se ha generado una devolución con el correlativo {devolucion.Correlativo} para el cliente {devolucion.CodigoCliente} por el motivo de {motivoDetalle.Descripcion} ", correos);
                    }

                    return Ok(devolucion.Correlativo);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("parcial")]
        public async Task<IHttpActionResult> PostDevolucionParcial([FromBody] List<DevolucionPostModel> devoluciones)
        {
            try
            {
                try
                {
                    var json = new JavaScriptSerializer().Serialize(devoluciones);

                    EscribirLogDevolucion($"Devolucion At: {DateTime.Now} : {json}.\n");
                }
                catch (Exception)
                {

                }

                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    Usuarios usuario = await ctx.Usuarios.FindAsync(user.Id);
                    Clientes cliente = await ctx.Clientes.FindAsync(devoluciones[0].CodigoCliente);
                    List<Object> nuevasDevoluciones = new List<Object>();
                    
                    var motivoDetalle = await ctx.MotivosDevolucionDetalle.FindAsync(devoluciones[0].MotivoDevolucionDetalle);
                    List<MotivosDevConAprobacion> AprobadoresSinFactura = new List<MotivosDevConAprobacion>();
                    var empresa = devoluciones[0].Empresa;
                    var motivoSinFactura = await ctx.MotivosDevolucion.FirstOrDefaultAsync(x => x.CodigoMotivoDevolucion == "SIN-FACTURA" && x.EmpresaId == empresa);
                    var minutosConf = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "TiempoFlotante");
                    int minutosValue = 2;
                   

                    if (minutosConf != null)
                    {
                        try
                        {
                            int.TryParse(minutosConf.Valor, out minutosValue);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (motivoSinFactura != null)
                    {
                        AprobadoresSinFactura = await ctx.MotivosDevConAprobacion.Where(x => x.IdMotivoDevolucion == motivoSinFactura.IdMotivoDevolucion && x.Estado == true).ToListAsync();
                    }

                    foreach (DevolucionPostModel devolucion in devoluciones)
                    {
                        var usuariosCorreo = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == usuario.Id && x.EmpresaId == devolucion.Empresa).Select(x => x.UsuarioId).ToListAsync();
                        var correos = await ctx.Usuarios.Where(x => x.CorreoDevolucion == true && x.Correo != null && usuariosCorreo.Contains(x.Id)).Select(x => x.Correo).ToListAsync();
                        var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId == usuario.EmpresaId);
                        var PendienteAprobacion = await ctx.MotivosDevConAprobacion.Where(x => x.IdMotivoDevolucion == devolucion.MotivoDevolucion && x.Estado == true).ToListAsync();
                        Devolucion found = null;
                        var fechaDesde = DateTime.Now.AddMinutes(Convert.ToDouble(minutosValue * -1));
                        var totalUnidades = devolucion.DetalleDevolucion.Sum(x => decimal.Parse(x.Cantidad.ToString()));
                        var totalPedido = devolucion.SubTotal;
                        var ubicacion = await ctx.UbicacionesXAlmacen.FirstOrDefaultAsync(x => x.MaestroBodegaAlmacenes.Almacen == devolucion.Almacen && x.MaestroBodegaAlmacenes.EmpresaId == devolucion.Empresa && x.ActivoDevolucion == true);

                        found = ctx.Devolucion.FirstOrDefault(x => (x.FechaCrea >= fechaDesde && x.FechaCrea <= DateTime.Now)
                                                                    && x.CodigoCliente == devolucion.CodigoCliente
                                                                    && x.Subtotal == totalPedido
                                                                    && x.TotalUnidades == totalUnidades
                                                                    && x.CodigoAsesor == user.UserAccount);

                        if (found == null)
                        {
                            found = ctx.Devolucion.FirstOrDefault(x => x.NumDevolucion == devolucion.Correlativo);
                        }

                        if (found == null)
                        {

                            Devolucion devolucionDB = new Devolucion()
                            {
                                NumDevolucion = devolucion.Correlativo,
                                CodigoCliente = devolucion.CodigoCliente,
                                IdLinea = devolucion.Linea,
                                IdMotivoDevDetalle = devolucion.MotivoDevolucionDetalle,
                                EmpresaId = devolucion.Empresa,
                                PedidoOrigen = devolucion.PedidoOriginal,
                                FacturaOrigen = devolucion.FacturaOriginal,
                                CodigoAsesor = cliente.CodigoAsesor,
                                UsuarioCrea = user.Id,
                                FechaCrea = DateTime.Now,
                                Sincronizado = false,
                                Estado = PendienteAprobacion.Count > 0 ? "Pendiente Aprobacion" : "No Sincronizado",
                                Subtotal = devolucion.SubTotal,
                                TotalUnidades = 0,
                                almacen = devolucion.Almacen,
                                IdUbicacion = ubicacion.UbicacionId
                            };

                            foreach (DevolucionDetallePostModel detalle in devolucion.DetalleDevolucion)
                            {
                                devolucionDB.TotalUnidades += detalle.Cantidad;

                                devolucionDB.DevolucionDetalle.Add(new DevolucionDetalle()
                                {
                                    NumDevolucion = devolucion.Correlativo,
                                    IdProducto = detalle.IdProducto,
                                    CodigoColor = detalle.CodigoColor,
                                    CodigoTalla = detalle.CodigoTalla,
                                    Cantidad = detalle.Cantidad,
                                    PrecioUnitario = detalle.PrecioUnitario,
                                    MontoLinea = detalle.Cantidad * detalle.PrecioUnitario
                                });
                            }
                            bool guardadoExito = AsyncSqlInsert.IngresarDevolucion(devolucionDB, usuario.EmpresaId);

                            if (guardadoExito)
                            {
                                if (string.IsNullOrEmpty(devolucion.FacturaOriginal) || string.IsNullOrWhiteSpace(devolucion.FacturaOriginal))
                                {
                                    foreach (var x in AprobadoresSinFactura)
                                    {
                                        AprobacionDevoluciones aprobacionDevoluciones = new AprobacionDevoluciones()
                                        {
                                            IdUsuario = x.IdUsuario,
                                            NumDevolucion = devolucion.Correlativo,
                                            Estado = true,
                                            UsuarioCrea = user.Id,
                                            FechaCrea = DateTime.Now
                                        };
                                        ctx.AprobacionDevoluciones.Add(aprobacionDevoluciones);
                                        var result = await ctx.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    foreach (var x in PendienteAprobacion)
                                    {
                                        AprobacionDevoluciones aprobacionDevoluciones = new AprobacionDevoluciones()
                                        {
                                            IdUsuario = x.IdUsuario,
                                            NumDevolucion = devolucion.Correlativo,
                                            Estado = true,
                                            UsuarioCrea = user.Id,
                                            FechaCrea = DateTime.Now
                                        };
                                        ctx.AprobacionDevoluciones.Add(aprobacionDevoluciones);
                                        var result = await ctx.SaveChangesAsync();
                                    }
                                }

                                if (!string.IsNullOrEmpty(devolucion.FacturaOriginal) || !string.IsNullOrWhiteSpace(devolucion.FacturaOriginal))
                                {
                                    ReducirPendienteDevolucion(devolucionDB);
                                }
                            }

                            _ = new Email().EnviarEmail($"Se ha generado una devolución con el correlativo {devolucion.Correlativo} para el cliente {devolucion.CodigoCliente} por el motivo de {motivoDetalle.Descripcion}", correos);
                        }

                        nuevasDevoluciones.Add(new { referencia = devolucion.Correlativo, factura = devolucion.FacturaOriginal });

                    }

                    return Ok(nuevasDevoluciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private async void ReducirPendienteDevolucion(Devolucion pedido)
        {
            using (var ctx = new AVentasEntities())
            {
                var lineasPedido = pedido.DevolucionDetalle;
                var PedidoOriginal = await ctx.PedidosxCliente.FirstOrDefaultAsync(x => x.NumeroPedido == pedido.PedidoOrigen && x.EmpresaId == pedido.EmpresaId);

                if (PedidoOriginal != null)
                {
                    foreach (var linea in lineasPedido)
                    {
                        var talla = linea.CodigoTalla.ToUpper();
                        var fisico = await ctx.PedidosDetalle.FirstOrDefaultAsync(x => x.PedidoId==PedidoOriginal.PedidoId && x.CodigoColor==linea.CodigoColor && x.CodigoTalla.ToUpper() == talla && x.IdProducto==linea.IdProducto);
                        fisico.CantidadDevolucion = fisico.CantidadDevolucion - linea.Cantidad;
                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }


        [HttpGet]
        [Route("obtencionFacturas/{codigoProducto}/{cliente}")]
        public async Task<IHttpActionResult> ObtenerFacturasPorProducto(string codigoProducto, string cliente)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var Facturas = ctx.SP_ObtencionFacturas(codigoProducto, cliente).ToList();
                    return Ok(Facturas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("reporte/{devolucion}")]
        public async Task<IHttpActionResult> GetReporteDevolucion(string devolucion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var detalleDevolucion = await ctx.DevolucionDetalle.Where(x => x.NumDevolucion == devolucion).Select(x => new
                    {
                        Producto = x.ProductosxColeccion.CodigoProducto,
                        Color = x.CodigoColor,
                        Talla = x.CodigoTalla,
                        Cantidad = x.Cantidad,
                    }).ToListAsync();

                    return Ok(detalleDevolucion);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private void EscribirLogDevolucion(string Message)
        {
            try
            {
                #region Creacion Carpeta
                string path = @"C:\AVentasAPIDevoluciones";
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
