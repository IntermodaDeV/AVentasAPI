using System.ComponentModel;

namespace ApiTrasladoService.Traslado.Models.Enum
{
    public enum SalesStatusEnum
    {
        [Description("PedidoAbierto")]
        PedidoAbierto = 1,
        [Description("Cancelado")]
        Cancelado = 4,
    }
}
