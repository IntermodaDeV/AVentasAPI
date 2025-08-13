using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    using System.Collections.Generic;
    public class IMTrasladoPedidoLineaDto
    {
        public string ITEMID { get; set; } = string.Empty;

        public string INVENTCOLORID { get; set; } = string.Empty;

        public string INVENTSIZEID { get; set; } = string.Empty;

        public decimal REMAININVENTPHYSICAL { get; set; }
    }
}
