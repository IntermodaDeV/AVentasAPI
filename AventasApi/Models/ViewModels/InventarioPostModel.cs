using System.Collections.Generic;

namespace AventasApi.Models.ViewModels
{
    public class InventarioPostModel
    {
        public InventarioPostModel()
        {
            DetalleInventario = new List<InventarioDetallePostModel>();
            ProductosNoEncontrados = new List<CodigoBarrasViewModel>();
        }
        public string CodigoCliente { get; set; }
        public string Correlativo { get; set; }
        public string Empresa { get; set; }
        public bool Completo { get; set; }
        public List<InventarioDetallePostModel> DetalleInventario;
        public List<CodigoBarrasViewModel> ProductosNoEncontrados;
    }
}