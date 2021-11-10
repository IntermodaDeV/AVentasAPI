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
                using(AVentasConfigEntities ctx = new AVentasConfigEntities())
                {
                    var correoPrincipal = ctx.CONFIGURACIONES.Where(x => x.CODIGO == 1000).FirstOrDefault();
                    var emailCliente = ctx.CONFIGURACIONES.Where(x => x.CODIGO == 1001).FirstOrDefault();
                    var usuario = ctx.CONFIGURACIONES.Where(x => x.CODIGO == 1002).FirstOrDefault();
                    var password = ctx.CONFIGURACIONES.Where(x => x.CODIGO == 1003).FirstOrDefault();                   

                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress(correoPrincipal.VALOR);

                    foreach(string correo in correos)
                    {
                        mail.To.Add(correo);
                    }
                    
                    mail.Subject = "Nueva devolución registrada";
                    mail.Body = msj;

                    using (var smtpClient = new SmtpClient(emailCliente.VALOR))
                    {
                        smtpClient.Port = 587;
                        smtpClient.Credentials = new System.Net.NetworkCredential(usuario.VALOR, password.VALOR);
                        smtpClient.EnableSsl = true;
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