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
using AventasApi.Infrastructure;
using AventasApi.Models.Authentication;

namespace AventasApi.Controllers
{
    //[Auth]
    public class CorreoController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        [HttpPost]
        public IHttpActionResult Post([FromBody] MailViewModel correo)
        {
            string nombreCliente = context.Clientes.FirstOrDefault(cli => cli.CodigoCliente== correo.CodigoCliente).Nombre;

            var email = "aaron.lemus@cit.hn";
            var ps = "Karr9526";
            MailMessage msg = new MailMessage();

            if (correo.pdf.Length > 0)
            {
                string[] pdfArray = correo.pdf.Split(',');
                byte[] imagenBytes = new byte[0];
                imagenBytes = Convert.FromBase64String(pdfArray[1]);


                msg.Attachments.Add(new Attachment( new MemoryStream(imagenBytes),"pedido.pdf"));
            }
            msg.From = new MailAddress(email);
            msg.To.Add(new MailAddress("aaron.lemus@cit.hn"));
            msg.To.Add(new MailAddress("gustavo.martinez@cit.hn"));
            msg.To.Add(new MailAddress("gabriela.guerrero@cit.hn"));
            msg.To.Add(new MailAddress("carlos.morales@cit.hn"));
            msg.Subject = "Recibo Intermoda";
            msg.Body = "Estimado(a) Señor(a):"  +nombreCliente+", "+
                       Environment.NewLine+
                       "Adjunto encontrara su pedido de venta.";
            msg.IsBodyHtml = true;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.SubjectEncoding = System.Text.Encoding.Default;
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, ps);
            client.Port = 587; // 
            client.Host = "smtp.live.com";
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
            var email = "aaron.lemus@cit.hn";
            var ps = "Karr9526";
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
            client.Host = "smtp.live.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Send(msg);
        }
    }
}
