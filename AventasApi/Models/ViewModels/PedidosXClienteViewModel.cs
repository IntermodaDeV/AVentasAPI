using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class Location
    {

        public bool mocked { get; set; }
        public decimal? accuracy { get; set; }
        public decimal? altitude { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public string error { get; set; }

    }

    public class ProductosXDetPed
    {
        public List<ColoresXProdXDetPed> coloresXProdXDetPed;
        public int? IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Imagen { get; set; }


        public Nullable<decimal> TotalXProducto { get; set; }
        public Nullable<decimal> CantidadXProducto { get; set; }

        //public List<TallaViewModel> ListaTalla;
        public ProductosXDetPed()
        {
            coloresXProdXDetPed = new List<ColoresXProdXDetPed>();

        }
    }
    public class GruposTallaXDetPed
    {
        public string GrupoTalla { get; set; }

        public List<ProductosXDetPed> prodsXDetPed;
        public List<TallaViewModel> ListaTalla;

        public GruposTallaXDetPed()
        {
            prodsXDetPed = new List<ProductosXDetPed>();
            ListaTalla = new List<TallaViewModel>();

        }
    }
    public class ColoresXProdXDetPed
    {
        public string IdColor { get; set; }
        public string NombreColor { get; set; }
        public List<DetalleXPedidoViewModel> DetallesXPedido;
        public Nullable<decimal> PrecioXColor { get; set; }
        public Nullable<decimal> TotalXColor { get; set; }
        public Nullable<decimal> CantidadXColor { get; set; }

        public ColoresXProdXDetPed()
        {
            DetallesXPedido = new List<DetalleXPedidoViewModel>();

        }
    }

    public class PedidosXClienteViewModel
    {
        public string PedidoId { get; set; }
        //public string CodigoCliente { get; set; }
        public string CodigoColeccion { get; set; }
        public string AcuerdoVenta { get; set; }
        public string EmpresaId { get; set; }
        public string Usuario { get; set; }
        public System.DateTime? FechaActual { get; set; }
        public System.DateTime? FechaEntrega { get; set; }
        public string Observacion { get; set; }
        public string NumeroPedido { get; set; }
        //public string NombreCliente { get; set; }

        public Nullable<int> ClienteContadoId { get; set; }
        public string ModoVenta { get; set; }

        public string Firma { get; set; }
        public ClienteViewModel Cliente { get; set; }

        public Location location = new Location();

        public List<ProductosXPedidoViewModel> Productos;
        public List<GruposTallaXDetPed> gruposXDetPed;

        public int TipoVenta { get; set; }
        public TipoPedidoViewModel TipoPedido { get; set; }

        public LineaViewModel Linea { get; set; }
        public string NombreColeccion { get; set; }
        public Nullable<decimal> TotalUnidades { get; set; }
        public Nullable<decimal> TotalXPedido { get; set; }
        public Nullable<decimal> SubTotalXPedido { get; set; }
        public Nullable<decimal> Impuesto { get; set; }
        public Nullable<decimal> Flete { get; set; }
        public bool Sincronizado { get; set; }

        public PedidosXClienteViewModel()
        {
            Productos = new List<ProductosXPedidoViewModel>();
            gruposXDetPed = new List<GruposTallaXDetPed>();

        }
    }
}