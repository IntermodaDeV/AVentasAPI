using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ApiTrasladoService.Shared.Utils
{
    public class XmlSerialize
    {
        public static string SerializeToXmlWithoutDeclaration<T>(T obj)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false // Puedes poner `true` si quieres que se vea bonito
            };

            var stringWriter = new StringWriter();
            var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xmlSerializer.Serialize(xmlWriter, obj);
            return stringWriter.ToString();
        }
    }
}
