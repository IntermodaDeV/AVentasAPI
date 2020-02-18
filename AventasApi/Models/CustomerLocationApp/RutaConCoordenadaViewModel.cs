using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.CustomerLocationApp
{
    public class RutaConCoordenadaViewModel
    {
        public RutaConCoordenadaViewModel()
        {
            Clientes = new List<ClientesConCoordendasViewModel>();
        }   
        public string CodigoRuta { get; set; }
        public string EmpresaId { get; set; }
        public string Nombre { get; set; }
        public int NumeroCoordenadasTomadas { get; set; }
        public List<ClientesConCoordendasViewModel> Clientes;
    }
}