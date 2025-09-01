using System.Xml.Serialization;

namespace ApiTrasladoService.Traslado.Models.XmlDtos
{
    [XmlRoot("Lineas")]
    public class TrasladoLineasDto
    {
        [XmlElement("Linea")]
        public TrasladoLineaDto[] Lineas { get; set; }
    }

    public class TrasladoLineaDto
    {
        [XmlElement]
        public string ItemId { get; set; }

        [XmlElement]
        public string InventColorId { get; set; }

        [XmlElement]
        public string InventSizeId { get; set; }

        [XmlElement]
        public int qty { get; set; }
    }
}
