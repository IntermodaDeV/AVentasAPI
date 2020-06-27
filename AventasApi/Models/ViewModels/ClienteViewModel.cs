using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBData.Database;
using AventasApi.Models.ViewModels;

namespace AventasApi.Models
{
    public class ClienteViewModel
    {
        public string EmpresaId { get; set; }
        public string Codigo { get; set; } 
        public string Asesor { get; set; }
        public string Nombre { get; set; }
        public string Zona { get; set; }
        public string ComunidadAutonoma { get; set; }
        public string GrupoPrecio { get; set; }
        public string GrupoCliente { get; set; }
        public string Descuento { get; set; }
        public string Moneda { get; set; }
        public string Direccion { get; set; }
        public string Ruta { get; set; }
        public string CodigoRuta { get; set; }
        public string FacturacionEntrega { get; set; }
        public string GrupoImpuesto { get; set; }

        public string ModoEntrega { get; set; }
        //public List<string> CuentaCorriente;
        public List<ContactosxClienteViewModel> Contacto ;
        public List<string> PedidosAbierto ;
        public List<CuentaCorrienteViewModel> CuentaCorriente;
        public List<AcuerdoVentaViewModel> AcuerdosVenta;
        public List<PedidosXClienteViewModel> Pedido;
        public List<AnticiposViewModel> Recibo;
        public Nullable<decimal> Latitud { get; set; }
        public Nullable<decimal> Longitud { get; set; }
        public List<FacturasXClienteViewModel> Facturas;
        public List<AcuerdosXTipoPedidoViewModel> AcuerdosXTipoPedido;
        public double NumeroFacturasVencidas { get; set; }
        public decimal MontoFacturasVencidas { get; set; }
        public double NumeroFacturasXVencer { get; set; }
        public decimal MontoFacturasXVencer { get; set; }
        public Nullable<decimal> LimiteCredito { get; set; }
        public Nullable<decimal> CreditoDisponible { get; set; }
        public List<PResumenCredito_Result> Credito { get; set; }   
        public ClienteViewModel()
        {
            this.Contacto = new List<ContactosxClienteViewModel>();
            this.CuentaCorriente = new List<CuentaCorrienteViewModel>();
            this.AcuerdosVenta = new List<AcuerdoVentaViewModel>();
            this.PedidosAbierto = new List<string>();
            this.Facturas = new List<FacturasXClienteViewModel>();
            this.AcuerdosXTipoPedido = new List<AcuerdosXTipoPedidoViewModel>();
            this.Credito = new List<PResumenCredito_Result>();
        }
    }
}