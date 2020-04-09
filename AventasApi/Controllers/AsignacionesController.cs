using AventasApi.Filters;
using DBData.Database;
using AventasApi.Models.Authentication;
//using IMS.Tokens.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AventasApi.Models.ViewModels;

namespace AventasApi.Controllers
{
    //[Auth]
    public class AsignacionesController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpGet]
        public async Task<IHttpActionResult> GetAsignacionesXRango(DateTime FechaInicio, DateTime FechaFin)
        {
            FechaInicio = new DateTime(FechaInicio.Year, FechaInicio.Month, FechaInicio.Day);
            FechaFin = new DateTime(FechaFin.Year, FechaFin.Month, FechaFin.Day);
            FechaFin = FechaFin.AddDays(1);
            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            var user = new { UserAccount = "gmonrroy" };
            List<AsignacionesXFechaViewModel> asignacionesXFecha = new List<AsignacionesXFechaViewModel>();
            var asignaciones = context.AsignacionxAsesor
                .Where(axa =>
                    axa.CodigoAsesor == context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount).CodigoAsesor && axa.FechaAsignacion >= FechaInicio &&
                    axa.FechaAsignacion < FechaFin).Select(axa => new 
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
                        PrioridadAsignacion = axa.PrioridadAsignacion
                    })
                .OrderBy(axa => axa.HoraInicio).ToList();

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
                        ColorRelleno = asignacion.PrioridadAsignacion.ColorRelleno
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
                        ColorRelleno = asignacion.PrioridadAsignacion.ColorRelleno
                    });
                }
            }
            return Ok(asignacionesXFecha);
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetAsignacionesCorrespondientes()
        {
            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            var user = new { UserAccount = "gmonrroy" };
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
                       IdTipoVisita = axa.idTipoVisita
                   })
                .OrderBy(axa => axa.HoraInicio).ToList();

            return Ok(
                new AsignacionesXFechaViewModel
                {
                    fecha = DateTime.Today,
                    asignaciones = asignaciones
                }
                );
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] List<AsignacionesXFechaViewModel> asignacionesNuevas)
        {

            var user = new { UserAccount = "gmonrroy" };
            //var user = TokenService.Validate<UserAuthenticated>(Request.Headers.Authorization.Parameter);
            string codigoAsesor = context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount).CodigoAsesor;
            foreach (var asignacionNueva in asignacionesNuevas)
            {
                DateTime FechaInicio = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month,
                    asignacionNueva.fecha.Value.Day);
                DateTime FechaFin = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month,
                    asignacionNueva.fecha.Value.Day);
                FechaFin = FechaFin.AddDays(1);
                List<AsignacionxAsesor> asignaciones =
                    asignacionNueva.asignaciones.Select(asi => new AsignacionxAsesor
                    {
                        Fecha = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month, asignacionNueva.fecha.Value.Day),
                        FechaAsignacion = new DateTime(asignacionNueva.fecha.Value.Year, asignacionNueva.fecha.Value.Month, asignacionNueva.fecha.Value.Day),
                        CodigoCliente = asi.cliente,
                        CodigoAsesor = codigoAsesor,
                        HoraInicio = asi.HoraInicio,
                        HoraFinal = asi.HoraFin,
                        idPrioridad = asi.IdPrioridad,
                        idTipoVisita = asi.IdTipoVisita,
                        Observacion = asi.Observacion
                    }).ToList();
                var asignacionesDB = context.AsignacionxAsesor
                        .Where(axa =>
                            axa.CodigoAsesor == context.Asesores.FirstOrDefault(ase => ase.Usuario == user.UserAccount).CodigoAsesor && axa.FechaAsignacion >= FechaInicio &&
                            axa.FechaAsignacion < FechaFin);
                if (asignacionesDB.Count() > 0)
                {

                    context.AsignacionxAsesor.RemoveRange(asignacionesDB);
                }
                if (asignaciones.Count() > 0)
                {

                    context.AsignacionxAsesor.AddRange(asignaciones);
                }
                context.SaveChanges();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
