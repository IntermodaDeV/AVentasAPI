using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace AventasApi.Utils
{
    public class LogicValidation
    {
        public void EmailNotification(string entityName, string counter)
        {
            string[] data = counter.Split('-');
            var bodyEmail = "<p>Hola Dev! <br/><br/>APP Ventas te informa que se han realizado las siguientes acciones"
            + " en la Entidad: " + entityName + ". A continuación se muestran los detalles: <br/><br/> - Cantidad de " 
            + "Datos Actualizados: " + data[0] +  ". <br/> - Cantidad de Datos Insertados: " + data[1] + " <br/>" +
            " - Cantidad de Datos con error: " + data[2] + "<br/> - Fecha que se realizó: " + DateTime.Now + 
            "<br/>Saludos!</p>";

            MailMessage correo = new MailMessage
            {
                From = new MailAddress("developerbverde@gmail.com"),
                Subject = "Notificación APP Ventas",
                Body = bodyEmail.ToString(),
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.Default
            };

            correo.To.Add(new MailAddress("christian.sanchez@cit.hn"));
            SmtpClient mail = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("developerbverde@gmail.com", "BV.Admin19")
            };

            try { mail.Send(correo); }
            catch {  }
        }

        public void EmailNotificationWithCollection(string entityName, string counter, string collection)
        {
            string[] data = counter.Split('-');
            var bodyEmail = "<p>Hola Dev! <br/><br/>APP Ventas te informa que se han realizado las siguientes acciones"
            + " en la Entidad: " + entityName + ". A continuación se muestran los detalles: <br/><br/> - Cantidad de "
            + "Datos Actualizados: " + data[0] + ". <br/> - Cantidad de Datos Insertados: " + data[1] + " <br/>" +
            " - Cantidad de Datos con error: " + data[2] + "<br/> - Identificador de la Colección: " + collection +
            "<br/> - Fecha que se realizó: " + DateTime.Now + "<br/>Saludos!</p>";

            MailMessage correo = new MailMessage
            {
                From = new MailAddress("developerbverde@gmail.com"),
                Subject = "Notificación APP Ventas",
                Body = bodyEmail.ToString(),
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.Default
            };

            correo.To.Add(new MailAddress("christian.sanchez@cit.hn"));
            SmtpClient mail = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("developerbverde@gmail.com", "BV.Admin19")
            };

            try { mail.Send(correo); }
            catch { }
        }

        public bool IsDataValid(object data)
        {
            var isValid = data != null;
            return isValid;
        }

        public bool ValidateDataCount(int counter)
        {
            int restrictionValue = 0;
            var isValid = counter > restrictionValue;
            return isValid;
        }

        public bool ValidateDataCountWithRestriction(int counter, int restrictionValue)
        {
            var isValid = counter > restrictionValue;
            return isValid;
        }
    }
}