using System.Collections.Generic;

namespace AventasApi.Models.ViewModels
{
    public class DevolucionPostModel
    {
        public DevolucionPostModel()
        {
            DetalleDevolucion = new List<DevolucionDetallePostModel>();
        }
        public string CodigoCliente { get; set; }
        public string Correlativo { get; set; }
        public string Moneda { get; set; }
        public int MotivoDevolucionDetalle { get; set; }
        public int MotivoDevolucion { get; set; }
        public string FacturaOriginal { get; set; }
        public string FacturaDestino { get; set; }
        public string PedidoOriginal { get; set; }
        public string Linea { get; set; }
        public string Empresa { get; set; }
        public decimal SubTotal { get;  set; }
        public string Almacen { get; set; }

        public List<DevolucionDetallePostModel> DetalleDevolucion;
    }
}