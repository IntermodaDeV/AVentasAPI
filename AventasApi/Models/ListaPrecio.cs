using System.Collections.Generic;

namespace AventasApi.Models
{
    public class ListaPrecio
    {
        public List<string> ListaPrecios { get; set; }
        public List<string> Paises { get; set; }
        public bool UsuarioOficina { get; set; }
    }
}