using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class DireccionesxClienteViewModel
    {
        public int id { get; set; }
        public string codigoCliente { get; set; }
        public long postalAddress { get; set; }
        public string nombreDireccion { get; set; }
        public string direccion { get; set; }
        public bool activo { get; set; }
        public bool principal { get; set; }
        public System.DateTime fechaCreacion { get; set; }
    }
}