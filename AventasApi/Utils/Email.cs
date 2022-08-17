using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DBData.Database;

namespace AventasApi.Utils
{
    public class Email
    {
        public async Task EnviarEmail(string msj,List<string>correos)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var correoPrincipal = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "CorreoPrincipal").FirstOrDefault();
                    var usuario = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "UsuarioCorreo").FirstOrDefault();
                    var password = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "CredencialCorreo").FirstOrDefault();
                    var port = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "MailPort").FirstOrDefault();
                    var host = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "Host").FirstOrDefault();

                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress(correoPrincipal.Valor);

                    foreach (string correo in correos)
                    {
                        mail.To.Add(correo);
                    }

                    mail.Subject = "Nueva devolución registrada";
                    mail.Body = msj;

                    using (var smtpClient = new SmtpClient(correoPrincipal.Valor))
                    {
                        smtpClient.Host = host.Valor;
                        smtpClient.Port = Convert.ToInt32(port.Valor);
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new System.Net.NetworkCredential(usuario.Valor, password.Valor);

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