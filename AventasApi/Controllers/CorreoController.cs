using AventasApi.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using AventasApi.Models.ViewModels;
//using IMS.Tokens.Services;
using DBData.Database;
using AventasApi.Models.Authentication;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Drawing;
using AventasApi.Services.Authentication;

namespace AventasApi.Controllers
{
    //[Auth]
    public class CorreoController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        AVentasEntities context = new AVentasEntities();

        public CorreoController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] MailViewModel correo)
        {
            string nombreCliente = context.Clientes.FirstOrDefault(cli => cli.CodigoCliente == correo.CodigoCliente).Nombre;

            var email = "soportecrmweb@gmail.com";
            var ps = "Intermoda1234";
            MailMessage msg = new MailMessage();

            if (correo.pdf.Length > 0)
            {
                string[] pdfArray = correo.pdf.Split(',');
                byte[] imagenBytes = new byte[0];
                imagenBytes = Convert.FromBase64String(pdfArray[1]);


                msg.Attachments.Add(new Attachment(new MemoryStream(imagenBytes), "pedido.pdf"));
            }
            msg.From = new MailAddress(email);
            msg.To.Add(new MailAddress("soportecrmweb@gmail.com"));
            msg.Subject = "Recibo Intermoda";
            msg.Body = "Estimado(a) Señor(a):" + nombreCliente + ", " +
                       Environment.NewLine +
                       "Adjunto encontrara su pedido de venta.";
            msg.IsBodyHtml = true;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.SubjectEncoding = System.Text.Encoding.Default;
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, ps);
            client.Port = 587; // 
            client.Host = "smtp.gmail.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Send(msg);
            return StatusCode(HttpStatusCode.NoContent);
        }
        private void SendEmail(List<string> ListRecievers, string body, string subject)
        {
            //Email Sender Info
            //var email = context.Configuraciones.FirstOrDefault(x => x.NombreConfiguracion == "email").Valor;
            //var ps = context.Configuraciones.FirstOrDefault(x => x.NombreConfiguracion == "emailps").Valor;
            var email = "soportecrmweb@gmail.com";
            var ps = "Intermoda1234";
            MailMessage msg = new MailMessage();
            foreach (var item in ListRecievers)
            {
                msg.To.Add(new MailAddress(item));
            }
            msg.From = new MailAddress(email);
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = true;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.SubjectEncoding = System.Text.Encoding.Default;
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, ps);
            client.Port = 587; // 
            client.Host = "smtp.gmail.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Send(msg);
        }

   
        #region ApiModulos

        // POST: api/mailmodulos
        [HttpPost]
        [Route("api/mailmodulos/crear")]
        public async Task<IHttpActionResult> PostMailModulo([FromBody] MailModulosViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailModulo = new MailModulos
                    {
                        ModuloId = model.ModuloId,
                        Descripcion = model.Descripcion,
                        FechaCreacion = model.FechaCreacion
                    };

                    ctx.MailModulos.Add(mailModulo);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Receptor creado exitosamente" });
                }

            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
            {
                // Clave duplicada (violación de restricción de unicidad)
                return BadRequest("El valor del ModuloId ya existe. Por favor, elige un ModuloId diferente.");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
            {
                // Otra excepción de clave duplicada
                return BadRequest("El valor del ModuloId ya existe. Por favor, elige un ModuloId diferente.");
            }
            catch (DbUpdateException ex)
            {
                // Error al actualizar la base de datos
                return BadRequest($"Error al actualizar la base de datos: {ex.Message}");
            }
            catch (SqlException ex)
            {
                // Error relacionado con SQL Server
                return BadRequest($"Error en la base de datos: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                // Argumento nulo
                return BadRequest($"Argumento nulo: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Operación no válida
                return BadRequest($"Operación no válida: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción no manejada
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }


        }


        [HttpGet]
        [Route("api/mailmodulos/listar")]
        public IHttpActionResult GetMailModulos()
        {
            using (var ctx = new AVentasEntities())
            {

                // Recupera solo los módulos sin incluir la relación MailServicios
                var mailModulosData = ctx.MailModulos
                    .Select(m => new
                    {
                        ModuloId = m.ModuloId,
                        Descripcion = m.Descripcion,
                        FechaCreacion = m.FechaCreacion
                    })
                    .ToList();


                /*var mailModulosData = ctx.MailModulos
                .Include(m => m.MailServicios)
                .Select(m => new
                {
                    ModuloId = m.ModuloId,
                    Descripcion = m.Descripcion,
                    FechaCreacion = m.FechaCreacion,
                    MailServicios = m.MailServicios.Select(s => new
                    {
                        ServicioID = s.ServicioID,
                        Descripcion = s.Descripcion
                    }).ToList()
                })
                .ToList();*/

                return Ok(mailModulosData);
            }
        }

        // GET
        [HttpGet]
        [Route("api/mailmodulos/listar/{id}")]
        public IHttpActionResult GetMailModuloById(string id)
        {
            using (var ctx = new AVentasEntities())
            {
                var mailModulo = ctx.MailModulos
                    .Include(m => m.MailServicios)
                    .Where(m => m.ModuloId == id)
                    .Select(m => new
                    {
                        ModuloId = m.ModuloId,
                        Descripcion = m.Descripcion,
                        FechaCreacion = m.FechaCreacion,
                        MailServicios = m.MailServicios.Select(s => new
                        {
                            ServicioID = s.ServicioID,
                            Descripcion = s.Descripcion
                        }).ToList()
                    })
                    .FirstOrDefault();

                if (mailModulo == null)
                {
                    return NotFound();
                }

                return Ok(mailModulo);
            }
        }

        // PUT
        [HttpPut]
        [Route("api/mailmodulos/actualizar/{id}")]
        public IHttpActionResult UpdateMailModulo(string id, MailModulosViewModel updatedMailModulo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var ctx = new AVentasEntities())
            {
                // Buscar el módulo con el ID proporcionado
                var mailModulo = ctx.MailModulos.FirstOrDefault(m => m.ModuloId == id);

                if (mailModulo == null)
                {
                    return NotFound();
                }

                // Actualizar las propiedades del módulo
                mailModulo.Descripcion = updatedMailModulo.Descripcion;
                mailModulo.FechaCreacion = updatedMailModulo.FechaCreacion;

                // Guardar los cambios en la base de datos
                ctx.SaveChanges();

                // Proyectar los datos actualizados en un ViewModel para evitar problemas de serialización
                var result = new
                {
                    mailModulo.ModuloId,
                    mailModulo.Descripcion,
                    mailModulo.FechaCreacion
                };

                return Ok(result);
            }
        }

        // DELETE
        [HttpDelete]
        [Route("api/mailmodulos/borrar/{id}")]
        public async Task<IHttpActionResult> DeleteMailModulo(String id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailModulo = await ctx.MailModulos.FindAsync(id);
                    if (mailModulo == null)
                    {
                        return NotFound(); // El módulo no existe
                    }

                    ctx.MailModulos.Remove(mailModulo);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Módulo eliminado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al actualizar la base de datos
                return BadRequest($"Error al actualizar la base de datos: {ex.Message}");
            }
            catch (SqlException ex)
            {
                // Error relacionado con SQL Server
                return BadRequest($"Error en la base de datos: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                // Argumento nulo
                return BadRequest($"Argumento nulo: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Operación no válida
                return BadRequest($"Operación no válida: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción no manejada
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }
        }

        #endregion ApiModulos

        #region ApiMailServicios
        [HttpPost]
        [Route("api/mailservicios/crear")]
        public async Task<IHttpActionResult> CrearMailServicio([FromBody] MailServiciosViewModel body)
        {

            if (string.IsNullOrEmpty(body.Modulo) || string.IsNullOrEmpty(body.ServicioID))
            {
                return BadRequest("Modulo y ServicioID son obligatorios.");
            }

            try
            {
                using (var ctx = new AVentasEntities())
                {

                    var moduloExists = await ctx.MailModulos
                                        .AnyAsync(m => m.ModuloId == body.Modulo);

                    if (!moduloExists)
                    {
                        return BadRequest("El valor del módulo no es válido. Por favor, asegúrate de que el módulo existe en el sistema.");
                    }


                    var mailServicio = new MailServicios
                    {
                        Modulo = body.Modulo,
                        ServicioID = body.ServicioID,
                        Descripcion = body.Descripcion,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = body.UsuarioCreacion,
                        Header = body.Header,
                        ValidaType = body.ValidaType,
                        Consulta = body.Consulta,
                        Footer = body.Footer,
                        Estado = body.Estado,
                        valida_empresaid = body.valida_empresaid
                    };

                    ctx.MailServicios.Add(mailServicio);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro creado exitosamente" });
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
            {
                // El número de error 2627 es para violación de la restricción de unicidad (clave duplicada)
                return BadRequest("El valor del ServicioID ya existe. Por favor, elige un ServicioID diferente.");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
            {
                // El número de error 2601 también es relacionado con clave duplicada
                return BadRequest("El valor del ServicioID ya existe. Por favor, elige un ServicioID diferente.");
            }
            catch (DbUpdateException ex)
            {
                // Esta excepción se lanza si ocurre un problema al actualizar la base de datos.
                return BadRequest($"Error al actualizar : {ex.Message}");
            }
            catch (SqlException ex)
            {
                // Esta excepción se lanza si ocurre un error relacionado con SQL Server.
                return BadRequest($"Error : {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                // Esta excepción se lanza si se pasa un argumento nulo a un método que no lo permite.
                return BadRequest($"Argumento nulo: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Esta excepción se lanza si el estado actual de un objeto no es válido para la operación solicitada.
                return BadRequest($"Operación no válida: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Captura cualquier otra excepción que no haya sido específicamente manejada.
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("api/mailservicios/obtener/{modulo}/{servicioID}")]
        public async Task<IHttpActionResult> ObtenerMailServicio(string modulo, string servicioID)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailServicio = await ctx.MailServicios
                        .Where(ms => ms.Modulo == modulo && ms.ServicioID == servicioID)
                        .Select(ms => new
                        {
                            ms.Modulo,
                            ms.ServicioID,
                            ms.Descripcion,
                            ms.FechaCreacion,
                            ms.UsuarioCreacion,
                            ms.Header,
                            ms.ValidaType,
                            ms.Consulta,
                            ms.Footer,
                            ms.Estado,
                            ms.valida_empresaid
                        })
                        .FirstOrDefaultAsync();

                    if (mailServicio == null)
                        return NotFound();

                    return Ok(mailServicio);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("api/mailservicios/obtener/{modulo}")]
        public async Task<IHttpActionResult> ObtenerMailServicioPorModulo(string modulo)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailServicio = await ctx.MailServicios
                        .Where(ms => ms.Modulo == modulo)
                        .Select(ms => new
                        {
                            ms.Modulo,
                            ms.ServicioID,
                            ms.Descripcion,
                            ms.FechaCreacion,
                            ms.UsuarioCreacion,
                            ms.Header,
                            ms.ValidaType,
                            ms.Consulta,
                            ms.Footer,
                            ms.Estado,
                            ms.valida_empresaid
                        })
                        .ToListAsync()
                        ;

                    if (mailServicio == null)
                        return NotFound();

                    return Ok(mailServicio);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("api/mailservicios/listar")]
        public async Task<IHttpActionResult> ListarMailServicios()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailServicios = await ctx.MailServicios
                        .Select(ms => new
                        {
                            ms.Modulo,
                            ms.ServicioID,
                            ms.Descripcion,
                            ms.FechaCreacion,
                            ms.UsuarioCreacion,
                            ms.Estado
                        })
                        .ToListAsync();

                    return Ok(mailServicios);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPut]
        [Route("api/mailservicios/actualizar")]
        public async Task<IHttpActionResult> ActualizarMailServicio([FromBody] MailServiciosViewModel body)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailServicio = await ctx.MailServicios
                        .FirstOrDefaultAsync(ms =>  ms.ServicioID == body.ServicioID);

                    if (mailServicio == null)
                        return NotFound();

                    mailServicio.Descripcion = body.Descripcion;
                    mailServicio.Header = body.Header;
                    mailServicio.ValidaType = body.ValidaType;
                    mailServicio.Consulta = body.Consulta;
                    mailServicio.Footer = body.Footer;
                    mailServicio.Estado = body.Estado;
                    mailServicio.valida_empresaid = body.valida_empresaid;

                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro actualizado exitosamente" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpDelete]
        [Route("api/mailservicios/eliminar/{modulo}/{servicioID}")]
        public async Task<IHttpActionResult> EliminarMailServicio(string modulo, string servicioID)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailServicio = await ctx.MailServicios
                        .FirstOrDefaultAsync(ms => ms.Modulo == modulo && ms.ServicioID == servicioID);

                    if (mailServicio == null)
                        return NotFound();

                    ctx.MailServicios.Remove(mailServicio);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro eliminado exitosamente" });
                }
            }
            catch (Exception e)
            {
                return BadRequest("No se pudo realizar eliminacion.");
            }
        }
        #endregion ApiMailServicios

        #region ApiMailReceptors 

        [HttpPost]
        [Route("api/mailreceptors/crear")]
        public async Task<IHttpActionResult> CrearMailReceptor([FromBody] MailReceptorsViewModel model)
        {
            if (model == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            try
            {
                using (var ctx = new AVentasEntities())
                {

                    var servicioExiste = await ctx.MailServicios.AnyAsync(s => s.ServicioID == model.ServicioID);

                    if (!servicioExiste)
                    {
                        return BadRequest("El ServicioID proporcionado no existe en la tabla MailServicios.");
                    }

                    var empresaExiste = await ctx.Empresa.AnyAsync(s => s.EmpresaId == model.EmpresaId);

                    if (!empresaExiste)
                    {
                        return BadRequest("La EmpresaID proporcionada no existe en la tabla Empresas.");
                    }


                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var mailReceptor = new MailReceptors
                    {
                        ServicioID = model.ServicioID,
                        EmpresaId = model.EmpresaId,
                        CorreoElectronico = model.CorreoElectronico,
                        FechaCreacion = DateTime.Now, 
                        UsuarioCreacion = user.UserAccount,
                        Estado = model.Estado,
                        FechaModifiacion = model.FechaModifiacion
                    };

                    ctx.MailReceptors.Add(mailReceptor);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Receptor creado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627) // 2627 es el código de error para clave duplicada
                {
                    return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
                }

                return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/mailreceptors/listar/{servicioID}")]
        public async Task<IHttpActionResult> ListarMailReceptores(string servicioID)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);


                using (var ctx = new AVentasEntities())
                {
                    var query = ctx.MailReceptors.AsQueryable();

                    if (!string.IsNullOrEmpty(servicioID))
                    {
                        query = query.Where(r => r.ServicioID == servicioID);
                    }
           
                    query = query.OrderBy(r => r.EmpresaId);

                    var receptores =  query.ToList();

                    return Ok(receptores);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Route("api/mailreceptors/actualizar")]
        public async Task<IHttpActionResult> ActualizarMailReceptor(
                                         [FromBody] MailReceptorsViewModel model)
        {
            if (model == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            if (string.IsNullOrWhiteSpace(model.ServicioID) || string.IsNullOrWhiteSpace(model.EmpresaId) || string.IsNullOrWhiteSpace(model.CorreoElectronico))
            {
                return BadRequest("Datos inválidos");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var mailReceptor = await ctx.MailReceptors
                        .FirstOrDefaultAsync(r => r.ServicioID == model.ServicioID
                                                  && r.EmpresaId == model.EmpresaId
                                                  && r.CorreoElectronico == model.CorreoElectronico);

                    if (mailReceptor == null)
                    {
                        return NotFound();
                    }

                    mailReceptor.UsuarioCreacion = user.UserAccount;
                    mailReceptor.FechaModifiacion = DateTime.Now;
                    mailReceptor.Estado = model.Estado;

                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Estado del receptor actualizado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
                }

                return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpDelete]
        [Route("api/mailreceptors/eliminar")]
        public async Task<IHttpActionResult> EliminarMailReceptor([FromBody] MailReceptorsViewModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailReceptor = await ctx.MailReceptors
                        .FirstOrDefaultAsync(r => r.ServicioID == model.ServicioID
                                                  && r.EmpresaId == model.EmpresaId
                                                  && r.CorreoElectronico == model.CorreoElectronico);

                    if (mailReceptor == null)
                    {
                        return NotFound();
                    }

                    ctx.MailReceptors.Remove(mailReceptor);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Receptor eliminado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion ApiMailReceptors

        #region ApiMailProExe

        [HttpPost]
        [Route("api/mailproexe/crear")]
        public async Task<IHttpActionResult> CrearMailProExe([FromBody] MailProExeViewModel model)
        {
            if (model == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }


            try
            {
                using (var ctx = new AVentasEntities())
                {

                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    var mailProExe = new MailProExe
                    {
                        ServicioID = model.ServicioID,
                        ProximaEjecucion = model.ProximaEjecucion,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = user.UserAccount,
                        IntervalType = model.IntervalType,
                        IntervalValue = model.IntervalValue
                    };

                    ctx.MailProExe.Add(mailProExe);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro creado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627) // 2627 es el código de error para clave duplicada
                {
                    return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
                }

                return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/mailproexe/listar")]
        public async Task<IHttpActionResult> ListarMailProExes()
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                using (var ctx = new AVentasEntities())
                {
                    var mailProExes = await ctx.MailProExe.ToListAsync();
                    return Ok(mailProExes);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/mailproexe/listar/{modulo}")]
        public async Task<IHttpActionResult> ListarMailProExesModulo(String modulo)
        {
            try
            {
                var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                using (var ctx = new AVentasEntities())
                {
                    var mailProExes = await ctx.MailProExe
                        .Where(ms => ms.ServicioID == modulo).ToListAsync();

                    return Ok(mailProExes);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPut]
        [Route("api/mailproexe/actualizar")]
        public async Task<IHttpActionResult> ActualizarMailProExe([FromBody] MailProExeViewModel model)
        {
            if (model == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailProExe = await ctx.MailProExe.FindAsync(model.Id);

                    if (mailProExe == null)
                    {
                        return NotFound();
                    }

                    var user = _authenticationAppService.Validate(Request.Headers.Authorization.Parameter);

                    mailProExe.ServicioID = model.ServicioID;
                    mailProExe.ProximaEjecucion = model.ProximaEjecucion;
                    mailProExe.FechaCreacion = model.FechaCreacion;
                    mailProExe.UsuarioCreacion = user.UserAccount;
                    mailProExe.IntervalType = model.IntervalType.Trim(); 
                    mailProExe.IntervalValue = model.IntervalValue;

                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro actualizado exitosamente" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627) // 2627 es el código de error para clave duplicada
                {
                    return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
                }

                return BadRequest("Error al actualizar la base de datos: " + ex.InnerException.InnerException.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("api/mailproexe/eliminar")]
        public async Task<IHttpActionResult> EliminarMailProExe([FromBody] MailProExeViewModel model)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var mailProExe = await ctx.MailProExe.FindAsync(model.Id);

                    if (mailProExe == null)
                    {
                        return NotFound();
                    }

                    ctx.MailProExe.Remove(mailProExe);
                    await ctx.SaveChangesAsync();

                    return Ok(new { success = true, message = "Registro eliminado exitosamente" });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }



    #endregion ApiMailProExe
}
