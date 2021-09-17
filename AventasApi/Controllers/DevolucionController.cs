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
                       NumeroDevolucion = x.NumDevolucion,
                       CodigoCliente = x.Devolucion.CodigoCliente,
                       NombreCliente = x.Devolucion.Clientes.Nombre,
                       Linea = x.Devolucion.IdLinea,
                       Estado = x.Devolucion.Estado,
                       FacturaOrigen = x.Devolucion.FacturaOrigen,
                       PedidoOrigen = x.Devolucion.PedidoOrigen,
                       Usuario = x.Devolucion.Usuarios.usuario,
                       MotivoDevolucion = x.Devolucion.MotivosDevolucionDetalle.Descripcion
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
                    var listaDevoluciones = db.AprobacionDevoluciones.FirstOrDefault(x => x.IdDevAprobacion == idDevAprobacion); 

                    if(listaDevoluciones == null)
                    {
                        return BadRequest("No existe el registro");
                    }

                    listaDevoluciones.Aprobado = true;
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
        [Route("sincronizar/{devolucion}")]
        public async Task<IHttpActionResult> PostDevolucionPendiente(string devolucion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var devolucionDB = await ctx.Devolucion.FirstOrDefaultAsync(x => x.NumDevolucion == devolucion && x.Procesando == false);

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
                             UNIT = "Und"
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
                                devolucionDB.NumeroRMA = probando[0];
                                devolucionDB.PedidoDevolucion = probando[1];
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
        [Route("listado")]
        public IHttpActionResult ObtenerlistadoDevoluciones()
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var listaCitas = db.Devolucion.Where(x=>x.CodigoAsesor== user.UserAccount).Select(x => new DevolucionesViewModel
                    {
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.NumeroRMA,
                        PedidoDevolucion = x.PedidoDevolucion,
                        CodigoCliente = x.CodigoCliente,
                        NombreCliente = x.Clientes.Nombre,
                        motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        Estado = x.Estado
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
        [Route("listado/pendiente")]
        public IHttpActionResult ObtenerlistadoDevolucionesPendientes()
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var listaCitas = db.Devolucion.Where(x => x.CodigoAsesor == user.UserAccount && x.Sincronizado==false).Select(x => new DevolucionesViewModel
                    {
                        NumDevolucion = x.NumDevolucion,
                        NumeroRMA = x.NumeroRMA,
                        PedidoDevolucion = x.PedidoDevolucion,
                        CodigoCliente = x.CodigoCliente,
                        NombreCliente = x.Clientes.Nombre,
                        motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                        Estado = x.Estado
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
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    Usuarios usuario = await ctx.Usuarios.FindAsync(user.Id);
                    Clientes cliente = await ctx.Clientes.FindAsync(devolucion.CodigoCliente);
                    var PendienteAprobacion = await ctx.MotivosDevConAprobacion.Where(x => x.IdMotivoDevolucion == devolucion.MotivoDevolucion && x.Estado == true).ToListAsync();
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
                        Estado = PendienteAprobacion.Count > 0 ? "Pendiente Aprobacion" : "No Sincronizado"
                    };

                    foreach(DevolucionDetallePostModel detalle in devolucion.DetalleDevolucion)
                    {
                        devolucionDB.DevolucionDetalle.Add(new DevolucionDetalle()
                        {
                            NumDevolucion=devolucion.Correlativo,
                            IdProducto=detalle.IdProducto,
                            CodigoColor=detalle.CodigoColor,
                            CodigoTalla=detalle.CodigoTalla,
                            Cantidad=detalle.Cantidad,
                            PrecioUnitario=detalle.PrecioUnitario
                        });
                    }
                    bool guardadoExito = AsyncSqlInsert.IngresarDevolucion(devolucionDB, usuario.EmpresaId);
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
                    ReducirPendienteDevolucion(devolucionDB);
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
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    Usuarios usuario = await ctx.Usuarios.FindAsync(user.Id);
                    Clientes cliente = await ctx.Clientes.FindAsync(devoluciones[0].CodigoCliente);

                    foreach(DevolucionPostModel devolucion in devoluciones)
                    {
                        var asesor = await ctx.Asesores.AsNoTracking().FirstOrDefaultAsync(ase => ase.Usuario == user.UserAccount && ase.EmpresaId == usuario.EmpresaId);
                        int numeroCorelativo = asesor.CorrelativoDevolucion ?? 0;
                        string inicialesAsesor = asesor.InicialesNombre;
                        string numeroReferencia = $"{inicialesAsesor}DEV-1{numeroCorelativo.ToString("D5")}";

                        Devolucion devolucionDB = new Devolucion()
                        {
                            NumDevolucion = numeroReferencia,
                            CodigoCliente = devolucion.CodigoCliente,
                            IdLinea = devolucion.Linea,
                            IdMotivoDevDetalle = devolucion.MotivoDevolucionDetalle,
                            EmpresaId = devolucion.Empresa,
                            PedidoOrigen = devolucion.PedidoOriginal,
                            FacturaOrigen = devolucion.FacturaOriginal,
                            CodigoAsesor = cliente.CodigoAsesor,
                            UsuarioCrea = user.Id,
                            FechaCrea = DateTime.Now,
                            Sincronizado = false
                        };

                        foreach (DevolucionDetallePostModel detalle in devolucion.DetalleDevolucion)
                        {
                            devolucionDB.DevolucionDetalle.Add(new DevolucionDetalle()
                            {
                                NumDevolucion = numeroReferencia,
                                IdProducto = detalle.IdProducto,
                                CodigoColor = detalle.CodigoColor,
                                CodigoTalla = detalle.CodigoTalla,
                                Cantidad = detalle.Cantidad,
                                PrecioUnitario = detalle.PrecioUnitario
                            });
                        }
                        bool guardadoExito = AsyncSqlInsert.IngresarDevolucion(devolucionDB, usuario.EmpresaId);
                        ReducirPendienteDevolucion(devolucionDB);
                    }

                    return Ok();
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
                var PedidoOriginal = await ctx.PedidosxCliente.FirstOrDefaultAsync(x => x.NumeroPedido == pedido.PedidoOrigen);

                if (PedidoOriginal != null)
                {
                    foreach (var linea in lineasPedido)
                    {
                        var talla = linea.CodigoTalla.Trim().ToUpper();
                        var fisico = await ctx.PedidosDetalle.FirstOrDefaultAsync(x => x.PedidoId==PedidoOriginal.PedidoId && x.CodigoColor==linea.CodigoColor && x.CodigoTalla.ToUpper() == talla && x.IdProducto==linea.IdProducto);
                        fisico.CantidadDevolucion = fisico.CantidadDevolucion - linea.Cantidad;
                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
