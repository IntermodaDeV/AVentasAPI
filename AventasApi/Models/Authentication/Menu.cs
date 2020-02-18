using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.Authentication
{
    public class Menu
    {
        public int IdMenu { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public string Icono { get; set; }
        public List<Menu> MenuHijos { get; set; }
        public string Por { get; set; }
    }
}