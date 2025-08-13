using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    public class IMTrasladoPedidoEncabezadoDto
    {
        public string SALESID { get; set; } = string.Empty;

        public string SALESNAME { get; set; } = string.Empty;
        public int SALESSTATUS { get; set; } 

        public string BFPSEASONID { get; set; } = string.Empty;

        public string NAME { get; set; } = string.Empty;
    }
}
