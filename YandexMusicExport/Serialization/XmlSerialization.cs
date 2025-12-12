using System.Text;
using System.Xml.Serialization;
using System.Xml;
using YandexMusicExport.Serialization.Models;

namespace YandexMusicExport.Serialization;

public static class XmlSerialization
{
    public static string XmlFileExport(string outputFileName, string directory, SerializablePlaylist playlist, Encoding encoding)
    {
        string outputFilePath = Path.Combine(directory, $"{outputFileName}.xml");
        using StreamWriter fs = new(outputFilePath, new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
        });
        SerializeXml(fs, playlist, encoding);
        return outputFilePath;
    }

    public static void SerializeXml<T>(StreamWriter stream, T data, Encoding encoding)
        where T : class
    {
        XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
        XmlSerializer xmlSerializer = new(typeof(T));
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = encoding
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        xmlSerializer.Serialize(writer, data, emptyNamespaces);
    }
}
