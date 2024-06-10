namespace AventasApi.Models.ViewModels
{
    public class DevolucionDetallePostModel
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoColor { get; set; }
        public int Cantidad { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string CodigoTalla { get; set; }
        public string Factura { get; set; }
        public string Pedido { get; set; }
        public string Paquete { get; set; }
    }
}