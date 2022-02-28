using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using AventasApi.Models;
using System.Globalization;
using System.Data.Entity;

namespace AventasApi.Controllers
{
    public class AsignacionMovil
    {
        public string CodigoAsesor { get; set; }
        public string CodigoCliente { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFinal { get; set; }
        public int Prioridad { get; set; }
    }
    //[Auth]
    public class AsignacionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        private readonly AuthenticationAppService _authenticationAppService;
        public AsignacionesController()
        {
            _authenticationAppService = new AuthenticationAppService();

        }
        [HttpGet]
        public async Task<IHttpActionResult> GetAsignacionesXRango(DateTime FechaInicio, DateTime FechaFin, string Asesor)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    List<AsignacionesXFechaViewModel> asignacionesXFecha = new List<AsignacionesXFechaViewModel>();
                    FechaInicio = new DateTime(FechaInicio.Year, FechaInicio.Month, FechaInicio.Day);
                    FechaFin = new DateTime(FechaFin.Year, FechaFin.Month, FechaFin.Day);
                    FechaFin = FechaFin.AddDays(1);
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                    List<string> asesoresHabilitados = new List<string>();
                    var usuario = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id);
                    var empresas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        if (Asesor != "null")
                        {
                            asesoresHabilitados = await ctx.Asesores.Where(x => x.CodigoAsesor == Asesor && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                        }
                        else
                        {
                            asesoresHabilitados = await ctx.Asesores.Where(x => empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                        }
                    }
                    else
                    {
                        var asesores = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();
                        asesoresHabilitados = await ctx.Asesores.Where(x => asesores.Contains(x.CodigoAsesor) && empresas.Contains(x.EmpresaId) && x.Activo == true).Select(x => x.CodigoAsesor).ToListAsync();
                    }

                    foreach (var asesor in asesoresHabilitados.Distinct().ToList())
                    {
                        var asignaciones = context.AsignacionxAsesor.Where(axa => axa.CodigoAsesor == asesor && axa.FechaAsignacion >= FechaInicio && axa.FechaAsignacion < FechaFin).Select(axa => new
                            {
                                IdAsignacionxAsesor = axa.IdAsignacionxAsesor,
                                Fecha = axa.Fecha,
                                CodigoCliente = axa.CodigoCliente,
                                CodigoAsesor = axa.CodigoAsesor,
                                Usuario = axa.Usuario,
                                FechaAsignacion = axa.FechaAsignacion,
                                Orden = axa.Orden,
                                HoraInicio = axa.HoraInicio,
                                HoraFinal = axa.HoraFinal,
                                idPrioridad = axa.idPrioridad,
                                idTipoVisita = axa.idTipoVisita,
                                Observacion = axa.Observacion,
                                PrioridadAsignacion = axa.PrioridadAsignacion,
                                Checkin = axa.BloqueoCheckin,
                                Checkout = axa.BloqueoCheckout,
                                Cancelada = axa.Cancelada,
                                Deshabilitada = axa.Deshabilitada
                            }).OrderBy(axa => axa.HoraInicio).ToList();

                    
                    foreach (var asignacion in asignaciones)
                        {
                            int indexAsignacionXFecha = asignacionesXFecha.FindIndex(axf => axf.fecha.Value.Year == asignacion.FechaAsignacion.Value.Year && axf.fecha.Value.DayOfYear == asignacion.FechaAsignacion.Value.DayOfYear);

                            if (indexAsignacionXFecha == -1)
                            {
                                AsignacionesXFechaViewModel nuevaAsignacionXFecha = new AsignacionesXFechaViewModel
                                {
                                    fecha = new DateTime(asignacion.FechaAsignacion.Value.Year, asignacion.FechaAsignacion.Value.Month,
                                        asignacion.FechaAsignacion.Value.Day),
                                    asignaciones = new List<AsignacionXAsesorViewModel>()
                                };
                                nuevaAsignacionXFecha.asignaciones.Add(new AsignacionXAsesorViewModel
                                {
                                    IdAsignacionxAsesor = asignacion.IdAsignacionxAsesor,
                                    cliente = asignacion.CodigoCliente,
                                    HoraInicio = asignacion.HoraInicio,
                                    HoraFin = asignacion.HoraFinal,
                                    IdPrioridad = asignacion.idPrioridad,
                                    IdTipoVisita = asignacion.idTipoVisita,
                                    Observacion = asignacion.Observacion,
                                    ColorRelleno = asignacion.PrioridadAsignacion.ColorRelleno,
                                    Checkin = asignacion.Checkin,
                                    Checkout = asignacion.Checkout,
                                    Asesor = asignacion.CodigoAsesor,
                                    Cancelada=asignacion.Cancelada,
                                    Deshabilitada=asignacion.Deshabilitada.Value
                                });
                                asignacionesXFecha.Add(nuevaAsignacionXFecha);
                            }
                            else
                            {
                                asignacionesXFecha[indexAsignacionXFecha].asignaciones.Add(new AsignacionXAsesorViewModel
                                {
                                    IdAsignacionxAsesor = asignacion.IdAsignacionxAsesor,
                                    cliente = asignacion.CodigoCliente,
                                    HoraInicio = asignacion.HoraInicio,
                                    HoraFin = asignacion.HoraFinal,
                                    IdPrioridad = asignacion.idPrioridad,
                                    IdTipoVisita = asignacion.idTipoVisita,
                                    Observacion = asignacion.Observacion,
                                    ColorRelleno = asignacion.PrioridadAsignacion.ColorRelleno,
                                    Checkin = asignacion.Checkin,
                                    Checkout = asignacion.Checkout,
                                    Asesor = asignacion.CodigoAsesor,
                                    Cancelada = asignacion.Cancelada,
                                    Deshabilitada = asignacion.Deshabilitada.Value
                                });
                            }
                        }
                    }
                    return Ok(asignacionesXFecha);
                }
                
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
            
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetAsignacionesCorrespondientes()
        {
            try
            {
                //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                //var user = new { UserAccount = "gmonrroy" };
                DateTime fechaFin = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day + 1);
                var asignaciones = context.AsignacionxAsesor
                    .Where(axa =>
                        axa.CodigoAsesor == context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount).CodigoAsesor && axa.FechaAsignacion >= DateTime.Today &&
                        axa.FechaAsignacion < fechaFin).Select(axa =>
                       new AsignacionXAsesorViewModel
                       {
                           IdAsignacionxAsesor = axa.IdAsignacionxAsesor,
                           cliente = axa.CodigoCliente,
                           HoraInicio = axa.HoraInicio,
                           HoraFin = axa.HoraFinal,
                           IdPrioridad = axa.idPrioridad,
                           IdTipoVisita = axa.idTipoVisita,
                           Checkin = axa.fechaCheckIn != null,
                           Checkout = axa.fechaCheckOut != null
                       })
                    .OrderBy(axa => axa.HoraInicio).ToList();

                return Ok(
                    new AsignacionesXFechaViewModel
                    {
                        fecha = DateTime.Today,
                        asignaciones = asignaciones
                    }
                    );
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
            
        }

        [HttpGet]
        [Route("~/api/asignaciones/reporte/{cliente}")]
        public IHttpActionResult GetReporteAsignaciones(string cliente)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<VisitasCliente_Result> reporte = ctx.VisitasCliente(cliente).ToList();
                    return Ok(reporte);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/asignaciones/reporte/{asesor}")]
        public IHttpActionResult GetReporteAsignacionesAsesor(string asesor, DateTime inicio, DateTime final)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<VisitasClientePorAsesor_Result> reporte = ctx.VisitasClientePorAsesor(asesor, inicio, final).ToList();
                    return Ok(reporte);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/asignaciones/reporte")]
        public IHttpActionResult GetReporteAsignacionesGlobal(DateTime inicio, DateTime final)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<VisitasClienteGlobal_Result> reporte = ctx.VisitasClienteGlobal(inicio, final).ToList();
                    return Ok(reporte);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private bool GuardarAsignaciones(List<AsignacionxAsesor> asignacionxAsesor)
        {
            try
            {
                using(AVentasEntities ctx = new AVentasEntities())
                {
                    if (asignacionxAsesor.Count() > 0)
                    {
                        ctx.AsignacionxAsesor.AddRange(asignacionxAsesor);
                        int guardadas = ctx.SaveChanges();

                        if (guardadas > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }catch(Exception e)
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] List<AsignacionesXFechaViewModel> asignacionesNuevas)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<AsignacionxAsesor> ListAsignaciones = new List<AsignacionxAsesor>();
                    foreach (var asignacionNueva in asignacionesNuevas)
                    {
                        DateTime FechaInicio = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month,
                            asignacionNueva.fecha.Value.Day);
                        DateTime FechaFin = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month,
                            asignacionNueva.fecha.Value.Day);
                        FechaFin = FechaFin.AddDays(1);
                        List<AsignacionxAsesor> asignaciones = asignacionNueva.asignaciones.Select(asi => new AsignacionxAsesor
                        {
                            Fecha = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month, asignacionNueva.fecha.Value.Day),
                            FechaAsignacion = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month, asignacionNueva.fecha.Value.Day),
                            CodigoCliente = asi.cliente,
                            CodigoAsesor = asi.Asesor,
                            HoraInicio = asi.HoraInicio,
                            HoraFinal = asi.HoraFin,
                            idPrioridad = asi.IdPrioridad,
                            idTipoVisita = asi.IdTipoVisita,
                            Observacion = asi.Observacion,
                            BloqueoCheckin = false,
                            BloqueoCheckout = true
                        }).ToList();

                        foreach (var asignacion in asignaciones)
                        {
                            var asignacionesDB = ctx.AsignacionxAsesor.Where(axa => axa.CodigoAsesor == asignacion.CodigoAsesor && axa.FechaAsignacion >= FechaInicio && axa.FechaAsignacion < FechaFin).ToList();

                            var ListAsignacionesDB = asignacionesDB.Where(axa => axa.HoraInicio == asignacion.HoraInicio && axa.HoraFinal == asignacion.HoraFinal).ToList();

                            if (ListAsignacionesDB.Count == 0)
                            {
                                ListAsignaciones.Add(asignacion);
                            }
                        }
                        GuardarAsignaciones(ListAsignaciones);
                    }

                    return StatusCode(HttpStatusCode.NoContent);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/asignaciones/checkin")]
        public async Task<IHttpActionResult> PostCheckin([FromBody] CheckInViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("No se puede registrar el checkin ya que los datos se encuentran vacios.");
                }

                using (var ctx = new AVentasEntities())
                {
                    var asignacion = ctx.AsignacionxAsesor.FirstOrDefault(x => x.IdAsignacionxAsesor == model.IdAsignacionxAsesor);
                    asignacion.latitudeCheckIn = (model.location != null) ? model.location.latitude : null;
                    asignacion.longitudeCheckIn = (model.location != null) ? model.location.longitude : null;
                    asignacion.fechaCheckIn = model.Fecha;
                    asignacion.BloqueoCheckin = true;
                    asignacion.BloqueoCheckout = false;

                    await ctx.SaveChangesAsync();

                    var FechaInicio = new DateTime(model.Inicio.Year, model.Inicio.Month, model.Inicio.Day);
                    var FechaFin = new DateTime(model.Fin.Year, model.Fin.Month, model.Fin.Day);
                    FechaFin = FechaFin.AddDays(1);

                    var asignaciones = await ctx.AsignacionxAsesor.Where(axa =>
                        axa.CodigoAsesor == model.Asesor 
                        && axa.FechaAsignacion >= FechaInicio 
                        && axa.FechaAsignacion < FechaFin 
                        && axa.IdAsignacionxAsesor != model.IdAsignacionxAsesor
                        && axa.fechaCheckIn == null
                        && axa.Cancelada == false
                    ).ToListAsync();

                    foreach (var tarea in asignaciones)
                    {
                        tarea.BloqueoCheckin = true;
                        tarea.Deshabilitada = true;
                        await ctx.SaveChangesAsync();
                    }

                    var response = new { Message = "Se ha registrado el checkin con exito." };
                    return Ok(response);
                }
            }catch(Exception e)
            {
                return BadRequest("Ocurrio un error, no se pudo registrar el checkin.");
            }
        }

        [HttpPost]
        [Route("~/api/asignaciones/eliminar/{id}")]
        public async Task<IHttpActionResult> EliminarAsignacion(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var asignacion = await ctx.AsignacionxAsesor.FindAsync(id);

                    if (asignacion == null)
                    {
                        return BadRequest("No se pudo eliminar la asignación,porque no existe.");
                    }

                    if(asignacion.BloqueoCheckin)
                    {
                        return BadRequest("No se puede eliminar asignación, ya se ha registrado el checkin.");
                    }

                    ctx.AsignacionxAsesor.Remove(asignacion);
                    int resultado = await ctx.SaveChangesAsync();

                    return Ok(resultado);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/asignaciones/checkout")]
        public async Task<IHttpActionResult> PostCheckout([FromBody] CheckInViewModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var asignacion = ctx.AsignacionxAsesor.FirstOrDefault(x => x.IdAsignacionxAsesor == model.IdAsignacionxAsesor);
                    asignacion.latitudeCheckOut = (model.location != null) ? model.location.latitude : null;
                    asignacion.longitudeCheckOut = (model.location != null) ? model.location.longitude : null;
                    asignacion.fechaCheckOut = model.Fecha;
                    asignacion.BloqueoCheckout = true;

                    await ctx.SaveChangesAsync();

                    var FechaInicio = new DateTime(model.Inicio.Year, model.Inicio.Month, model.Inicio.Day);
                    var FechaFin = new DateTime(model.Fin.Year, model.Fin.Month, model.Fin.Day);
                    FechaFin = FechaFin.AddDays(1);

                    var asignaciones = await ctx.AsignacionxAsesor.Where(axa =>
                        axa.CodigoAsesor == model.Asesor 
                        && axa.FechaAsignacion >= FechaInicio 
                        && axa.FechaAsignacion < FechaFin 
                        && axa.IdAsignacionxAsesor!=model.IdAsignacionxAsesor
                        && axa.fechaCheckIn==null
                        && axa.Cancelada == false
                    ).ToListAsync();

                    foreach (var tarea in asignaciones)
                    {
                        tarea.BloqueoCheckin = false;
                        tarea.Deshabilitada = false;
                        await ctx.SaveChangesAsync();
                    }

                    var response = new { Message = "Se ha registrado el checkout con exito." };
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                return BadRequest("Ocurrio un error, no se pudo registrar el checkout.");
            }
        }

        [HttpPost]
        [Route("~/api/asignaciones/cargar")]
        public async Task<IHttpActionResult> CargarAsignaciones([FromBody] IEnumerable<AsignacionViewModel> model)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);
                
                if (model == null || model.Count() == 0)
                {
                    return BadRequest("No se puede registrar una lista vacia.");
                }

                using (var ctx = new AVentasEntities())
                {
                    var listaDominio  = ConvertirListaAsignacion(model);
                    var listaGuardar = ConvertirListaAsignacion(model);
                    var usuario  = await ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == user.Id); 
                    var asesores = listaDominio.Select(x => x.CodigoAsesor).Distinct().ToList();
                    var empresas = model.Select(x => x.Empresa).Distinct().ToList();
                    var empresasAsignadas = await ctx.Usuarios_Empresas.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.EmpresaId).ToListAsync();

                    if (usuario.FlagTodosAsesores.Value)
                    {
                        foreach (var empresa in empresas)
                        {
                            if (!empresasAsignadas.Contains(empresa))
                            {
                                return BadRequest($"La empresa {empresa} no esta asignada a su usuario.");
                            }
                        }
                    }
                    else
                    {
                        var asesoresAsignados = await ctx.Usuarios_Asesores.Where(x => x.Status == true && x.UsuarioId == user.Id).Select(x => x.CodigoAsesor).ToListAsync();

                        foreach (var asesor in asesores)
                        {
                            if (!asesoresAsignados.Contains(asesor))
                            {
                                return BadRequest($"El asesor {asesor} no esta asignado a su usuario.");
                            }
                        }

                        foreach (var empresa in empresas)
                        {
                            if (!empresasAsignadas.Contains(empresa))
                            {
                                return BadRequest($"La empresa {empresa} no esta asignada a su usuario.");
                            }
                        }
                    }

                    List<AsignacionxAsesor> listaGuardadas = new List<AsignacionxAsesor>();
                    List<int> indiceErrores = new List<int>();

                    for (int x = 0; x < listaDominio.Count(); x++)
                    {
                        var asignacion = listaDominio[x];

                        var entityFound = await ctx.Clientes.FirstOrDefaultAsync(cli => cli.CodigoCliente == asignacion.CodigoCliente && cli.CodigoAsesor == asignacion.CodigoAsesor);

                        if (entityFound == null)
                        {
                            //return BadRequest($"El cliente no existe o no esta asignado al asesor. En asignacion {x + 1}");
                            indiceErrores.Add(x);
                        }

                        if (asignacion.Fecha < DateTime.Today)
                        {
                            //return BadRequest($"Una o más asignaciones no se pueden crear ya que pertenecen a una fecha anterior. En asignacion {x + 1}");
                            indiceErrores.Add(x);
                        }
                    }

                    foreach (var asesor in asesores)
                    {
                        var entityFound = await ctx.Asesores.FirstOrDefaultAsync(cli => cli.CodigoAsesor == asesor);

                        if (entityFound == null)
                        {
                            return BadRequest($"El codigo de asesor no existe.");
                        }

                        var listaAsignacionPorAsesor = await ctx.AsignacionxAsesor
                            .Where(x=>x.CodigoAsesor== asesor)
                            .OrderByDescending(x => x.FechaAsignacion)
                            .Take(50)
                            .ToListAsync();

                        if (listaAsignacionPorAsesor.Count() > 0)
                        {
                            listaGuardadas.AddRange(listaAsignacionPorAsesor);
                        }
 
                    }
 
                    for(int x = 0; x < listaDominio.Count(); x++)
                    {
                        var asignacionp = listaDominio[x];
                        var listaComparacion = listaGuardadas.Where(cli=>cli.CodigoAsesor==asignacionp.CodigoAsesor).ToList();
                        //listaDominio.RemoveAt(x);
                        var asignacionesAsesor = listaDominio.Where(cli => cli.CodigoAsesor == asignacionp.CodigoAsesor);
                        listaComparacion.AddRange(asignacionesAsesor);

                        var indice = listaGuardar.FindIndex(cli => cli.CodigoAsesor == asignacionp.CodigoAsesor 
                            && cli.CodigoCliente==asignacionp.CodigoCliente
                            && cli.HoraInicio==asignacionp.HoraInicio
                            && cli.HoraFinal==asignacionp.HoraFinal
                        );

                        listaComparacion.Remove(asignacionp);

                        for (int y = 0; y < listaComparacion.Count(); y++)
                        {

                            if ((asignacionp.HoraInicio >= listaComparacion[y].HoraInicio) && (asignacionp.HoraInicio <= listaComparacion[y].HoraFinal) && asignacionp.CodigoAsesor == listaComparacion[y].CodigoAsesor)
                            {
                                var cliente = ctx.Clientes.FirstOrDefault(cli => cli.CodigoCliente == asignacionp.CodigoCliente);
                                indiceErrores.Add(indice);
                                //return BadRequest($"[Línea {indice + 1} - Asesor {asignacion.CodigoAsesor}] Una o más asignaciones tienen conflicto de horario para el cliente {asignacion.CodigoCliente} - {cliente.Nombre}.");
                            }

                            if ((asignacionp.HoraFinal >= listaComparacion[y].HoraInicio) && (asignacionp.HoraFinal <= listaComparacion[y].HoraFinal) && asignacionp.CodigoAsesor == listaComparacion[y].CodigoAsesor)
                            {
                                var cliente = ctx.Clientes.FirstOrDefault(cli => cli.CodigoCliente == asignacionp.CodigoCliente);
                                indiceErrores.Add(indice);
                                //return BadRequest($"[Línea {indice + 1} - Asesor {asignacion.CodigoAsesor}] Una o más asignaciones tienen conflicto de horario para el cliente {asignacion.CodigoCliente} - {cliente.Nombre}.");
                            }
                        }
                    }

                    if (indiceErrores.Count() > 0)
                    {
                        return Ok(new { StatusCode = 400, Lista = indiceErrores,Message="Una o más asignaciones no se pueden registrar por conflictos de horario o el cliente no pertenece al asesor." });
                    }

                    ctx.AsignacionxAsesor.AddRange(listaGuardar);
                    var result = await ctx.SaveChangesAsync();
                    var response = new { StatusCode=200,Message = $"Se han registrado {result} asignaciones." };
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                return BadRequest("Ha ocurrido un error y no se pudo registar las asignaciones.");
            }
        }

        private List<AsignacionxAsesor> ConvertirListaAsignacion(IEnumerable<AsignacionViewModel> model)
        {
            return model.Select(x => new AsignacionxAsesor()
            {
                Fecha = DateTime.ParseExact(x.FechaAsignacion, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                FechaAsignacion = DateTime.ParseExact(x.FechaAsignacion, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                CodigoAsesor = x.CodigoAsesor,
                idPrioridad = x.idPrioridad,
                HoraInicio = DateTime.ParseExact($"{x.FechaAsignacion} {x.HoraInicio}", "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                HoraFinal = DateTime.ParseExact($"{x.FechaAsignacion} {x.HoraFinal}", "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                CodigoCliente = x.CodigoCliente,
                BloqueoCheckin = false,
                BloqueoCheckout = true,
                Deshabilitada=false
            }).ToList();
        }

        [HttpPost]
        [Route("~/api/asignaciones/crear/movil")]
        public async Task<IHttpActionResult> CrearAsignacionMovil([FromBody] AsignacionMovil asignacion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                { 
                    List<AsignacionxAsesor> asignacionesDelDia = await ctx.AsignacionxAsesor
                        .Where(a => a.CodigoAsesor == asignacion.CodigoAsesor 
                        && a.FechaAsignacion.Value.Year == asignacion.FechaAsignacion.Year
                        && a.FechaAsignacion.Value.Month == asignacion.FechaAsignacion.Month
                        && a.FechaAsignacion.Value.Day == asignacion.FechaAsignacion.Day
                        && a.Cancelada == false
                        ).ToListAsync();

                    foreach(AsignacionxAsesor a in asignacionesDelDia)
                    {
                        if ((asignacion.HoraInicio >= a.HoraInicio) && (asignacion.HoraInicio <= a.HoraFinal)){
                            return BadRequest("La visita no puede ser creada porque tiene conflicto de horarios con otra visita.");
                        }

                        if ((asignacion.HoraFinal >= a.HoraInicio) && (asignacion.HoraFinal <= a.HoraFinal))
                        {
                            return BadRequest("La visita no puede ser creada porque tiene conflicto de horarios con otra visita.");
                        }
                    }

                    AsignacionxAsesor nuevaAsignacion = new AsignacionxAsesor
                    {
                        CodigoAsesor = asignacion.CodigoAsesor,
                        CodigoCliente = asignacion.CodigoCliente,
                        Fecha = asignacion.FechaAsignacion,
                        FechaAsignacion = asignacion.FechaAsignacion,
                        HoraInicio = asignacion.HoraInicio,
                        HoraFinal = asignacion.HoraFinal,
                        idPrioridad = asignacion.Prioridad,
                        BloqueoCheckin = false,
                        BloqueoCheckout = true,
                        Cancelada = false,
                        Deshabilitada=false
                    };

                    ctx.AsignacionxAsesor.Add(nuevaAsignacion);
                    int affectedRows = await ctx.SaveChangesAsync();

                    if (affectedRows > 0)
                    {
                        return Ok(new
                        {
                            IdAsignacionxAsesor = nuevaAsignacion.IdAsignacionxAsesor,
                            cliente = nuevaAsignacion.CodigoCliente,
                            HoraInicio = nuevaAsignacion.HoraInicio,
                            HoraFin = nuevaAsignacion.HoraFinal,
                            IdPrioridad = nuevaAsignacion.idPrioridad,
                            IdTipoVisita = nuevaAsignacion.idTipoVisita,
                            Observacion = nuevaAsignacion.Observacion,
                            Checkin = nuevaAsignacion.BloqueoCheckin,
                            Checkout = nuevaAsignacion.BloqueoCheckout,
                            Asesor = nuevaAsignacion.CodigoAsesor,
                            Cancelada = nuevaAsignacion.Cancelada,
                            Deshabilitada = nuevaAsignacion.Deshabilitada
                        });
                    }

                    return BadRequest("Ocurrio un error y no se pudo guardar la asignación");
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
