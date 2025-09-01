using ApiTrasladoService.Traslado.Models.XmlDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace AventasApi.Models.Traslado.XmlDtos
{
    [XmlRoot("Clientes")]
    public class ClientesDto
    {
        [XmlElement("Cliente")]
        public List<ClienteDto> Cliente { get; set; }
    }

    public class ClienteDto
    {
        public TrasladoEncabezadoDto Encabezado { get; set; }
        public TrasladoLineasDto Lineas { get; set; }
    }
}