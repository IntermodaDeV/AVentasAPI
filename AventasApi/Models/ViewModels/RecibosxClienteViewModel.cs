using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RecibosxClienteViewModel
    {
        public RecibosxClienteViewModel()
        {
            DetalleRecibo = new List<RecibosDetalleViewModel>();
        }
        public int ReciboId { get; set; }
        public string NumeroRecibo { get; set; }
        public string CodigoCliente { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<int> IdTipoPago { get; set; }
        public string Referencia { get; set; }
        public string DescripcionBanco { get; set; }
        public Nullable<System.DateTime> FechaPago { get; set; }
        public Nullable<int> IdBanco { get; set; }
        public Nullable<int> IdCuentaBancaria { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public string IdMoneda { get; set; }
        public Nullable<bool> Sincronizado { get; set; }
        public string CodigoAsesor { get; set; }
        public Nullable<int> IdFactura { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }

        public LocationCliente locationCliente = new LocationCliente();
        public TipoPagoViewModel TipoPago{ get; set; }
        public ClienteViewModel Cliente { get; set; }
        public PedidosXClienteViewModel Pedido { get; set; }
        public string SpecPago { get; set; }
        public string Asesor { get; set; }
        public string UsuarioCreacion { get; set; }
        public Nullable<DateTime> FechaCreacion { get; set; }
        public int? proformaId { get; set; }
        public string firma { get; set; }
        public byte[] firmaByte { get; set; }

        public List<RecibosDetalleViewModel> DetalleRecibo { get; set; }
        public bool Anticipo { get;  set; }
        public string NombreAsesor { get; internal set; }
        public int Estado { get; internal set; }
        public int Id { get; internal set; }
        public string ReciboGenerado { get; internal set; }
        public int NumeroCopia { get; internal set; }
        public string EmpresaUsuario { get; set; }
    }
}