
using AventasApi.Models.Traslado.DTOs;
using System.Web;

namespace ApiTrasladoService.Traslado.Models.DTOs
{
    public class SincronizarPlantillaAXRequestDto
    {
        public MemoryPostedFile Archivo { get; set; }
        public string NombreDelVendedor { get; set; }
        public string CodigoDelVendedor { get; set; }
        public string Company { get; set; }
    }
}
