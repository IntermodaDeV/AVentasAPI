using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class InventarioDetalleViewModel
    {
        public List<ProductosXPedidoViewModel> Productos;
        public List<GruposTallaXDetPed> gruposXDetPed;

        public InventarioDetalleViewModel()
        {
            Productos = new List<ProductosXPedidoViewModel>();
            gruposXDetPed = new List<GruposTallaXDetPed>();

        }
    }
}