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
        public string Nombre { get; set; }
        public string Moneda { get; set; }
        public string MotivoDevolucion { get; set; }
        public string MotivoDevolucionDetalle { get; set; }

        public List<DevolucionDetallePostModel> DetalleDevolucion;
    }
}