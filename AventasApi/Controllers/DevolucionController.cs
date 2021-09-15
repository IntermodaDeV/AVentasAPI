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
                        Sincronizado =false
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
