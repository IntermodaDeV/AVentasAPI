using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    public class IMTrasladoPedidoDto
    {
        public List<IMTrasladoPedidoMotivoDTO> Motivos { get; set; } 
        public IMTrasladoPedidoEncabezadoDto Encabezado { get; set; } 
        public List<IMTrasladoPedidoLineaDto> Lineas { get; set; } 
    }
}
