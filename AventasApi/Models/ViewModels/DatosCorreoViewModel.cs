using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DatosCorreoViewModel
    {
        public string usuario { get; set; }
        public string detalle { get; set; }
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
        public string rtn { get; set; }
        public byte[] Imagen { get; set; }
    }
}