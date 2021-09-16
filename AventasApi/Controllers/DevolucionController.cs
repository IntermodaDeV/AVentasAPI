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
                    var result = db.SaveChanges();
                    return Ok(result);
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
                        Estado = PendienteAprobacion.Count > 0 ? "Pendiente Aprobacion" : ""
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
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
