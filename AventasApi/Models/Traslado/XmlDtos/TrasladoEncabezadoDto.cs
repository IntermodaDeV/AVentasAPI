using System.Xml.Serialization;

namespace ApiTrasladoService.Traslado.Models.XmlDtos
{
    [XmlRoot("Encabezado")]
    public class TrasladoEncabezadoDto
    {
        [XmlElement]
        public string PedidoOrigen { get; set; }

        [XmlElement]
        public string Lote { get; set; }

        [XmlElement]
        public string Motivo { get; set; }

        [XmlElement]
        public string CuentaDeCliente { get; set; }

        [XmlElement]
        public string NombreDelVendedor { get; set; }

        [XmlElement]
        public string CodigoDelVendedor { get; set; }
    }
}
