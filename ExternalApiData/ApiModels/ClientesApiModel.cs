using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
 

    public class PedidoXCliente
    {
        public string pedidoId { get; set; } 
        public string acuerdoVentaId { get; set; } 
        public string cliente { get; set; } 
        public string empresaId { get; set; } 
        public System.DateTime? fecha { get; set; }
        public System.DateTime? fechaEntrega { get; set; }
        public string observacion { get; set; } 

        //falta detalle pedido
    } public class GrupoPrecio
    {
        public string codigo { get; set; } 
        public string description { get; set; } 
        public string description2 { get; set; } 

        //falta detalle pedido
    }
    public class ClientesApiModel
    {
        public string empresaId { get; set; }
        public string codigo { get; set; } 
        public string nombre { get; set; }
        public string zona { get; set; }
        public string comunidadAutonoma { get; set; }
        public string grupoPrecio { get; set; }
        public string grupoCliente { get; set; }
        public string descuento { get; set; }
        public string moneda { get; set; }
        public string ruta { get; set; }
        public string acuerdodeVenta { get; set; } 
        public string provincias { get; set; } 
        public string region { get; set; } 
        public string facturacionEntrega { get; set; }
        public decimal  latitud { get; set; }
        public decimal  longitud { get; set; }
        public decimal  precisionGPS { get; set; } //
        public List<string> rutas;
        //public List<PedidoXCliente> pedidos ;
        public List<CuentaCorrienteApiModel> listacuentacorriente ;
        public List<GrupoPrecio> listaGrupoPrecio ;
        //Falta zona


        public List<string> direcciones;

        
        public ClientesApiModel ()
        {
            this.direcciones = new List<string>();
            this.rutas = new List<string>();
            //this.pedidos = new List<PedidoXCliente>();
            this.listacuentacorriente = new List<CuentaCorrienteApiModel>();
            this.listaGrupoPrecio = new List<GrupoPrecio>();
        }
    }
}