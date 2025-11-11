using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AventasApi.Models.ViewModels
{
    public class AsesorPedidosDTO
    {
        public string CodigoAsesor { get; set; }
        public string PedidoDevolucion { get; set; }
        public string NumeroRMA { get; set; }
        public string EmpresaId { get; set; }
        public string NumPedido { get; set; }
        public string NumDevolucion { get; set; }
        public string Estado { get; set; }
    }
}