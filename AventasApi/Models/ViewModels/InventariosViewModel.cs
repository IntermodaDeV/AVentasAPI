using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class InventariosViewModel
    {
        public string numInventario { get; set; }
        public string cliente { get; set; }
        public string empresa { get; set; }
        public DateTime creado { get; set; }
        public DateTime modificado { get; set; }
        public bool completado { get; set; }
        public int unidades { get; set; }
    }
}