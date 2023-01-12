using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DevolucionesViewModel
    {
        public string NumDevolucion { get; set; }

        public string PedidoDevolucion { get; set; }

        public string NumeroRMA { get; set; }

        public string CodigoCliente { get; set; }

        public string NombreCliente { get; set; }

        public string IdLinea { get; set; }

        public string motivoDevolucion { get; set; }

        public string EmpresaId { get; set; }

        public string Observacion { get; set; }
        public string UsuarioModifica { get; set; }

        public string CodigoAsesor { get; set; }

        public string Estado { get; set; }
        public string Usuario { get; set; }

        public string ErrorAx { get; set; }

        public string Linea { get; set; }

        public int cantidad { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ClienteViewModel Cliente { get; set; }
        public int? TotalUnidades { get;  set; }
        public decimal? SubTotal { get;  set; }

        public int? EstadoBodega { get; set; }
    }
}