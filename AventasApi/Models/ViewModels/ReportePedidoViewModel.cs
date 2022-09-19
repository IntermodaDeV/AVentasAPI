namespace AventasApi.Models.ViewModels
{
    public class ReportePedidoViewModel
    {
        public string NumeroPedido { get; set; }
        public string codigoCliente { get; set; }
        public string nombre { get; set; }
        public string asesor { get; set; }
        public decimal? latitud { get; set; }
        public decimal? longitud { get; set; }
        public decimal? clienteLatitud { get; set; }
        public decimal? clienteLongitud { get; set; }
        public string distancia { get; set; }
        public string estado { get; set; }
    }
}
