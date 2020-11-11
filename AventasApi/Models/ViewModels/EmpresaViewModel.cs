using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class EmpresaViewModel
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        public string Telefono { get; set; }

        public string CorreoElectronico { get; set; }

        public string Logo { get; set; }

        public string Direccion { get; set; }

        public string RegistroTributario { get; set; }

        public Nullable<bool> Revision { get; set; }

        public string DocumentoFiscal { get; set; }
    }
}