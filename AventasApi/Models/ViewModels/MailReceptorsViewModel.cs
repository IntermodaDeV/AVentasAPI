using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class MailReceptorsViewModel
    {
        public string ServicioID { get; set; }
        public string EmpresaId { get; set; }
        public string CorreoElectronico { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaModifiacion { get; set; }
    }
}

