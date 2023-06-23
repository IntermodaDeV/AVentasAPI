using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Models;
using DBData.Database;
using AventasApi.Services.Authentication;
using AventasApi.Utils;
using AventasApi.Wrappers;

namespace AventasApi.Controllers
{
    public class PromesaPagoController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public PromesaPagoController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
        [HttpPost]
        [Route("~/api/promesaPago/crear")]
        public async Task<IHttpActionResult> CrearPromesaPago([FromBody] PromesaPagoModel datos)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var usuario = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var promesasBD = ctx.PromesaPago.Where(x => x.IdAsignacionXAsesor == datos.IdAsignacionXAsesor).ToList();
                    if(promesasBD.Count() > 0)
                    {
                        return BadRequest("Ya existe una promesa de pago registrada para la visita.");
                    }
                    var nuevaPromesa = new PromesaPago() {
                        IdAsignacionXAsesor = datos.IdAsignacionXAsesor, 
                        FechaPromesa = datos.FechaPromesa, 
                        Valor = datos.Valor,
                        UsuarioCrea = usuario.Id,
                        FechaCrea = DateTime.Now 
                    };
                    ctx.PromesaPago.Add(nuevaPromesa);

                    var guardarPromesa = await ctx.SaveChangesAsync();                    

                    if(guardarPromesa > 0)
                    {
                        var guardarAsignacion = await GuardarAsignacion(datos.FechaPromesa, datos.IdAsignacionXAsesor, usuario.UserAccount);

                        if(guardarAsignacion.Succeeded == true)
                        {                            
                            return Ok(new { msjPromesa = "Se ha registrado la promesa con exito.", msjAsignacion = guardarAsignacion.Message, success = guardarAsignacion.Succeeded });
                        }

                        return Ok(new { msjPromesa = "Se ha registrado la promesa con exito.", msjAsignacion = guardarAsignacion.Message, success = guardarAsignacion.Succeeded });

                    }

                    return Ok(new { msjPromesa = "Promesa no registrada.", msjAsignacion = "Tu cliente no pude ser programado de forma automática!", success = false });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        private async Task<Response>  GuardarAsignacion(DateTime fechaPromesa, int idAsignacionXAsesor, string usuario)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {

                    DateTime fecha = new DateTime(fechaPromesa.Year, fechaPromesa.Month, fechaPromesa.Day);
                    DateTime horaInicio = new DateTime(fechaPromesa.Year, fechaPromesa.Month, fechaPromesa.Day, 07, 00, 00);
                    DateTime horaFin = new DateTime(fechaPromesa.Year, fechaPromesa.Month, fechaPromesa.Day, 07, 30, 00);

                    var asignacionesDB = await ctx.AsignacionxAsesor.Where(x => x.CodigoAsesor == usuario && x.FechaAsignacion == fecha && x.HoraInicio >= horaInicio && x.HoraFinal <= horaFin).ToListAsync();
                    
                    if(asignacionesDB.Count > 0)
                    {
                        return new Response(false, $"Tu cliente no fue programado de manera automática en la fecha {horaInicio} en tu agenda de visita, debido a que ya cuentas con una asignación para esta fecha.");
                    }

                    var codigoCliente = ctx.AsignacionxAsesor.FirstOrDefault(x => x.IdAsignacionxAsesor == idAsignacionXAsesor).CodigoCliente;
                    var nombreCliente = ctx.Clientes.FirstOrDefault(x => x.CodigoCliente == codigoCliente).Nombre;

                    var asignacion = new AsignacionxAsesor
                    {
                        Fecha = fecha,
                        FechaAsignacion = fecha,
                        CodigoCliente = codigoCliente,
                        CodigoAsesor = usuario,
                        HoraInicio = horaInicio,
                        HoraFinal = horaFin,
                        idPrioridad = 1,
                        idTipoVisita = null,
                        Observacion = null,
                        BloqueoCheckin = false,
                        BloqueoCheckout = true,
                        Deshabilitada = false,
                        EsPromesaPago = true
                    };
                    ctx.AsignacionxAsesor.Add(asignacion);

                    int guardadas = ctx.SaveChanges();

                        if (guardadas > 0)
                        {
                            var correo = ctx.Usuarios.FirstOrDefault(x => x.usuario == usuario).Correo;
                         if(correo != null)                      
                            _ = new SendEmail().EmailSend(titulo: $"NOTIFICACION DE VISITA DE PROMESA DE PAGO CLIENTE {nombreCliente}", 
                                                          contenido: await createEmailBody(nombreCliente, horaInicio), 
                                                          correo: correo);

                        return new Response(true, $"Tu cliente fue programado de manera automática en la fecha {horaInicio} en tu agenda de visita."); 
                        }

                    return new Response(false, "Tu cliente no pude ser programado de forma automática."); 
                }
            }
            catch (Exception e)
            {
                return new Response(false, "Tu cliente no pude ser programado de forma automática!");
            }
        }

        private async Task<string> createEmailBody(string nombreCliente, DateTime horaInicio)
        {
            string body =  "<html>" +
                          "<head>" +
                            "<title>Email System</title>" +
                          "</head>" +
                          "<body>" +
                              "<table>" +
                                  "<tr>" +
                                      "<td>" +
                                      "<div style=\"border-top:3px solid #227de5\"> </div>" +
                                          "<span style=\"font-family:Arial;font-size:10pt\">" +
                                              $"<p>SE LE COMUNICA QUE SU CLIENTE <b>{nombreCliente}</b> ESTA PROGRAMADO DE FORMA AUTOMATICA EN FECHA " +
                                              $"<b>{horaInicio}</b> PARA HACER EFECTIVA LA PROMESA DE PAGO" +
                                              "</p>" +
                                              "<br />" +
                                              "<b>SALUDOS!</b>" +
                                              "<br />" +
                                          "</span>" +
                                          "<div style=\"border-top:3px solid #227de5\"> </div>" +
                                          "<br />" +
                                          "<br />" +
                                          "<br />" +
                                          "</td>" +
                                  "</tr>" +
                              "</table>" +
                          "</body>" +
                          "</html>";          
            return body;
        }

    }
}
