using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.CustomerLocationApp
{
    public class ClientesConCoordendasViewModel
    {
        public ClientesConCoordendasViewModel()
        {
            Coordenadas = new List<CoordenadasXClienteViewModel>();
        }
        public string CodigoCliente { get; set; }
        public string EmpresaId { get; set; }
        public string Nombre { get; set; }
        public string ComunidadAutonoma { get; set; }
        public string GrupoPrecio { get; set; }
        public string GrupoCliente { get; set; }
        public string Direccion { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string GrupoImpuesto { get; set; }
        public List<CoordenadasXClienteViewModel> Coordenadas;
    }
}