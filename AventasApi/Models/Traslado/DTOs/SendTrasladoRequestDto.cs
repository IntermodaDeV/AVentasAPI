using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    public class SendTrasladoRequestDto
    {
        public string pedido { get; set; }
        public string dataAreaId { get; set; }
        public string emailDestino { get; set; }
    }
}
