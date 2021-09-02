namespace AventasApi.Models.ViewModels
{
    public class DevolucionDetallePostModel
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoColor { get; set; }
        public int Cantidad { get; set; }
        public string Unidad { get; set; }
        public double PrecioUnitario { get; set; }
        public string Talla { get; set; }
    }
}