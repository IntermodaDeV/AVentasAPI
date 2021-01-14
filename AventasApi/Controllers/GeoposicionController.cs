using System;
using System.Linq;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Generic;
using System.Globalization;

namespace AventasApi.Controllers
{
    public class QueryFilter
    {
        public List<string> asesores { get; set; }
    }

    public class QueryFilterRecorrido
    {
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string asesor { get; set; }
    }
    public class GeoposicionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public GeoposicionController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpPost]
        public IHttpActionResult Post([FromBody] GoeposicionXAsesorViewModel geoposicion)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                context.BitacoraGeoposicion.Add(new BitacoraGeoposicion
                {
                    IdAsignacionxAsesor = geoposicion.IdAsignacionxAsesor,
                    Mocked = geoposicion.Mocked,
                    Accuracy = geoposicion.Accuracy,
                    Altitude = geoposicion.Altitude,
                    Latitude = geoposicion.Latitude,
                    Longitude = geoposicion.Longitude,
                    CodigoAsesor = user.UserAccount,
                    Fecha = DateTime.Now
                });
                context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/Geoposicion/EnviarUbicacion")]
        public IHttpActionResult PostServicio([FromBody] GoeposicionXAsesorViewModel geoposicion)
        {
            try
            {
                context.BitacoraGeoposicion.Add(new BitacoraGeoposicion
                {
                    IdAsignacionxAsesor = geoposicion.IdAsignacionxAsesor,
                    Mocked = geoposicion.Mocked,
                    Accuracy = geoposicion.Accuracy,
                    Altitude = geoposicion.Altitude,
                    Latitude = geoposicion.Latitude,
                    Longitude = geoposicion.Longitude,
                    CodigoAsesor = geoposicion.CodigoAsesor,
                    Fecha = DateTime.Now
                });
                context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Geoposicion/asesores")]
        public async Task<IHttpActionResult> ObtenerAsesores()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        var asesores = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId) && x.Activo==true)
                            .Select(x => new { codigo = x.CodigoAsesor, nombre = x.Nombre, empresa = x.EmpresaId }).ToListAsync();
                        return Ok(asesores);
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id ).Select(x => x.CodigoAsesor).ToListAsync();
                        var asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo == true)
                            .Select(x => new { codigo = x.CodigoAsesor, nombre = x.Nombre, empresa = x.EmpresaId }).ToListAsync();
                        return Ok(asesoresHabilitados);
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Geoposicion")]
        public IHttpActionResult ObtenerUbicacionAsesor([FromUri]QueryFilter filter)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<object> coordenadas = new List<object>();

                    foreach(var asesor in filter.asesores)
                    {
                        var ultimaCoordenada = ctx.BitacoraGeoposicion.Where(x => x.CodigoAsesor == asesor)
                        .OrderByDescending(x => x.Fecha)
                        .Take(1)
                        .Select(x => new { 
                            latitude = x.Latitude, 
                            longitude = x.Longitude,
                            ultimaFecha=x.Fecha,
                        }).FirstOrDefault();

                        if (ultimaCoordenada != null)
                        {

                            var asesorCoordenada = ctx.Asesores.FirstOrDefault(x => x.CodigoAsesor == asesor);
                            var ultimaAsignacion = ctx.AsignacionxAsesor.Where(x => x.CodigoAsesor == asesor && x.fechaCheckOut!=null).OrderByDescending(x => x.fechaCheckOut).Take(1).Select(x => new
                            {
                                codigoCliene = x.CodigoCliente,
                                fechaCheckout = x.fechaCheckOut
                            }).FirstOrDefault();

                            if (ultimaAsignacion != null)
                            {
                                var ultimoCliente = ctx.Clientes.FirstOrDefault(x => x.CodigoCliente == ultimaAsignacion.codigoCliene);
                                coordenadas.Add(new
                                {
                                    latitude = ultimaCoordenada.latitude,
                                    longitude = ultimaCoordenada.longitude,
                                    ultimaFecha = ultimaCoordenada.ultimaFecha,
                                    asesor = asesorCoordenada.Nombre,
                                    fechacheckout = ultimaAsignacion != null ? ultimaAsignacion.fechaCheckout : null,
                                    nombrecliente = ultimoCliente.Nombre,
                                    codigocliente = ultimoCliente.CodigoCliente

                                });
                            }
                            else
                            {
                                coordenadas.Add(new
                                {
                                    latitude = ultimaCoordenada.latitude,
                                    longitude = ultimaCoordenada.longitude,
                                    ultimaFecha = ultimaCoordenada.ultimaFecha,
                                    asesor = asesorCoordenada.Nombre,
                                    fechacheckout = ultimaAsignacion != null ? ultimaAsignacion.fechaCheckout : null,
                                    nombrecliente = "",
                                    codigocliente = ""

                                });
                            }
                        }
                    }

                    return Ok(coordenadas);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/Geoposicion/recorrido")]
        public IHttpActionResult ObtenerRecorridoAsesor([FromUri]QueryFilterRecorrido filter)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    var fechaInicio = DateTime.ParseExact($"{filter.FechaInicio}", "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
                    var fechaFin = DateTime.ParseExact($"{filter.FechaFin}", "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
                    var coordenadas = ctx.BitacoraGeoposicion.Where(x => x.CodigoAsesor == filter.asesor && x.Fecha>=fechaInicio && x.Fecha<=fechaFin)
                        .Select(x => new { 
                            lat = x.Latitude, 
                            lng = x.Longitude })
                        .ToList();

                    var pedidos = ctx.PedidosxCliente.Where(x => x.CodigoAsesor == filter.asesor && x.Fecha >= fechaInicio && x.Fecha <= fechaFin)
                        .Select(x => new
                        {
                            pedidoId=x.PedidoId,
                            numeroPedido=x.NumeroPedido,
                            cliente=x.CodigoCliente,
                            nombreCliente = x.Clientes.Nombre,
                            totalPedido=x.TotalPedido,
                            lat = x.Latitude,
                            lng = x.Longitude
                        }).ToList();

                    var recibos = ctx.RecibosxCliente.Where(x => x.CodigoAsesor == filter.asesor && x.Fecha >= fechaInicio && x.Fecha <= fechaFin)
                        .Select(x => new
                        {
                            reciboId = x.ReciboId,
                            numeroRecibo = x.NumeroRecibo,
                            cliente = x.CodigoCliente,
                            nombreCliente=ctx.Clientes.FirstOrDefault(c=>c.CodigoCliente==x.CodigoCliente).Nombre,
                            totalRecibo=x.Valor,
                            moneda=x.IdMoneda,
                            lat = x.Latitude,
                            lng = x.Longitude
                        }).ToList();

                    var datos = new { coordenadas=coordenadas,recibos=recibos,pedidos=pedidos };
                    return Ok(datos);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
