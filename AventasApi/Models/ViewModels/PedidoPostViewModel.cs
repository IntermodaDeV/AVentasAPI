using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PedidoPostViewModel
    {
        public string NumeroReferencia { get; set; }
        public string CodigoCliente { get; set; }
        public string CodigoColeccion { get; set; }
        public string AcuerdoVenta { get; set; }
        public string EmpresaId { get; set; }
        public string Usuario { get; set; }
        public System.DateTime? FechaActual { get; set; }
        public System.DateTime? FechaEntrega { get; set; }
        public string Observacion { get; set; }

        public string NombreCliente { get; set; }

        public string Firma { get; set; }

        public Location location = new Location();

        public List<DetallePedidoPostViewModel> DetallePedido;

        public int TipoVenta { get; set; }
        public TipoPedidoViewModel TipoPedido { get; set; }

        public string Linea { get; set; }
        public string NombreColeccion { get; set; }
        public Nullable<decimal> TotalUnidades { get; set; }
        public Nullable<decimal> TotalXPedido { get; set; }
        public decimal Flete { get; set; }

        public Nullable<int> ClienteContadoId { get; set; }
        public string ModoVenta { get; set; }
        public decimal Impuesto { get; set; }
        public bool RequiereEntrega { get; set; }
        public bool PedidoCache { get; set; }

        public PedidoPostViewModel()
        {
            DetallePedido = new List<DetallePedidoPostViewModel>();

        }
    }
}