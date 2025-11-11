using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AventasApi.Models.ViewModels
{
    public class AsesorEstadoDevolucionDTO
    {
        public string NumDevolucion { get; set; }
        public string NumPedido { get; set; }
        public string NumRMA { get; set; }
        public string Estado { get; set; }
    }
}