using DBData.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Interop;

namespace AventasApi.Utils
{
    public class SendEmail
    {
        public async Task EmailSend(string titulo, string contenido,  string correo )
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var correoPrincipal = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CorreoPrincipal").Valor;
                    var usuario = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "UsuarioCorreo").Valor;
                    var password = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CredencialCorreo").Valor;
                    var port = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "MailPort").Valor;
                    var host = ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "Host").Valor;

                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(correoPrincipal)
                    };
                    mail.To.Add(correo);                    

                    mail.Subject = titulo;
                    mail.Body = contenido;
                    mail.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(correoPrincipal))
                    {
                        smtpClient.Host = host;
                        smtpClient.Port = Convert.ToInt32(port);
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new System.Net.NetworkCredential(usuario, password);

                        await smtpClient.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}