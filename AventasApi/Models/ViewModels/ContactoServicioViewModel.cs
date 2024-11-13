using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ContactoServicioViewModel
    {
        public int Id { get; set; }
        public string IdEmpresa { get; set; }
        public string Telefono { get; set; }
        public string Whatsapp { get; set; }
        public string UrlQRWhatsapp { get; set; }
        public bool Activo { get; set; }
    }
}