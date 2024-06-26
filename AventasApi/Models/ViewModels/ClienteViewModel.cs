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
        public string Telefono { get; set; }
        public string ModoEntrega { get; set; }
        //public List<string> CuentaCorriente;
        public List<ContactosxClienteViewModel> Contacto;
        public List<DescuentoViewModel> MaestroDescuento;
        public List<string> PedidosAbierto;
        public List<CuentaCorrienteViewModel> CuentaCorriente;
        public List<AcuerdoVentaViewModel> AcuerdosVenta;
        public List<PedidosXClienteViewModel> Pedido;
        public List<AnticiposViewModel> Recibo;
        public Nullable<decimal> Latitud { get; set; }
        public Nullable<decimal> Longitud { get; set; }
        public List<FacturasXClienteViewModel> Facturas;
        public List<AcuerdosXTipoPedidoViewModel> AcuerdosXTipoPedido;

        public List<ChequesContabilizadosViewModel> chequesContabilizados;

        public string NombreGrupoPrecio { get; set; }
        public int DiasTransporte { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public string Alias { get; set; }

        public double NumeroFacturasVencidas { get; set; }
        public decimal MontoFacturasVencidas { get; set; }
        public double NumeroFacturasXVencer { get; set; }
        public decimal MontoFacturasXVencer { get; set; }
        public Nullable<decimal> LimiteCredito { get; set; }
        public Nullable<decimal> CreditoDisponible { get; set; }
        public List<PResumenCredito_Result> Credito { get; set; }
        public bool? IgnorarSecuenciaFactura { get; internal set; }
        public List<SP_DocumentosAplicadosXCuotas_Result> DocumentosAplicadosxCuotas { get; set; }
        public List<ReservadoClientePorLineaViewModel> ReservadoClientePorLinea { get; set; }

        public ClienteViewModel()
        {
            this.Contacto = new List<ContactosxClienteViewModel>();
            this.MaestroDescuento = new List<DescuentoViewModel>();
            this.CuentaCorriente = new List<CuentaCorrienteViewModel>();
            this.AcuerdosVenta = new List<AcuerdoVentaViewModel>();
            this.PedidosAbierto = new List<string>();
            this.Facturas = new List<FacturasXClienteViewModel>();
            this.AcuerdosXTipoPedido = new List<AcuerdosXTipoPedidoViewModel>();
            this.Credito = new List<PResumenCredito_Result>();
            this.chequesContabilizados = new List<ChequesContabilizadosViewModel>();
            this.DocumentosAplicadosxCuotas = new List<SP_DocumentosAplicadosXCuotas_Result>();
        }
    }

    public class DireccionesClienteViewModel
    {
        public long postalAddress { get; set; }
        public string nombreDireccion { get; set; }
        public string direccion { get; set; }
        public bool principal { get; set; }
    }

    public class ClientePedidoViewModel
    {
        public string EmpresaId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string ComunidadAutonoma { get; set; }
        public string GrupoPrecio { get; set; }
        public string NombreGrupoPrecio { get; set; }
        public string GrupoCliente { get; set; }
        public string Descuento { get; set; }
        public string Moneda { get; set; }
        public string Direccion { get; set; }
        public string FacturacionEntrega { get; set; }
        public string GrupoImpuesto { get; set; }
        public string ModoEntrega { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public string Alias { get; set; }

        public List<CuentaCorrienteViewModel> CuentaCorriente;

        public List<AcuerdoVentaViewModel> AcuerdosVenta;
        public Nullable<decimal> LimiteCredito { get; set; }
        public Nullable<decimal> CreditoDisponible { get; set; }
        public List<PResumenCredito_Result> Credito { get; set; }
        public decimal? Longitud { get; internal set; }
        public decimal? Latitud { get; internal set; }
        public bool IncluyeImpuesto { get; internal set; }
        public List<DireccionesClienteViewModel> Direcciones;
        public List<ReservadoClientePorLineaViewModel> ReservadoClientePorLinea { get; set; }
        public ClientePedidoViewModel()
        {
            this.Credito = new List<PResumenCredito_Result>();
            this.CuentaCorriente = new List<CuentaCorrienteViewModel>();
            this.Direcciones = new List<DireccionesClienteViewModel>();
        }
    }

    public class ClienteAgendaViewModel
    {
        public string EmpresaId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Zona { get; set; }
        public string ComunidadAutonoma { get; set; }
        public string Direccion { get; set; }
        public string Ruta { get; set; }
        public string CodigoRuta { get; set; }
        public string Moneda { get; set; }
        public string Asesor { get; set; }

        public Nullable<decimal> Latitud { get; set; }
        public Nullable<decimal> Longitud { get; set; }

        public List<FacturasXClienteViewModel> Facturas;

        public List<AcuerdosXTipoPedidoViewModel> AcuerdosXTipoPedido;
        public double NumeroFacturasVencidas { get; set; }
        public decimal MontoFacturasVencidas { get; set; }
        public double NumeroFacturasXVencer { get; set; }
        public decimal MontoFacturasXVencer { get; set; }

        public ClienteAgendaViewModel()
        {
            this.Facturas = new List<FacturasXClienteViewModel>();
            this.AcuerdosXTipoPedido = new List<AcuerdosXTipoPedidoViewModel>();
        }
    }
}

