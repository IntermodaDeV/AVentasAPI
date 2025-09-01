using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    public class DetallePedidoDto
    {
        public string Articulo { get; set; }
        public string Color { get; set; }
        public int Talla { get; set; }
        public int CantidadOrigen { get; set; }
    }
}
