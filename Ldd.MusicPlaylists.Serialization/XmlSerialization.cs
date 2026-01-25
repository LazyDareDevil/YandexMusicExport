using Ldd.MusicPlaylists.Serialization.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ldd.MusicPlaylists.Serialization;

public static class XmlSerialization
{
    public static bool TryXmlFileExport(string outputFilePath, SerializablePlaylist playlist, Encoding encoding)
    {
        try
        {
            using StreamWriter fs = new(outputFilePath, new FileStreamOptions()
            {
                Mode = FileMode.OpenOrCreate,
                Access = FileAccess.Write,
            });
            SerializeXml(fs, playlist, encoding);
            return true;
        }
        catch
        {
            return false;
        }
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

    public static bool TryLoadFromXml(string inputFilePath, [MaybeNullWhen(false)] out SerializablePlaylist playlist)
    {
        using FileStream fs = new(inputFilePath, FileMode.Open, FileAccess.Read);
        XmlSerializer xmlSerializer = new(typeof(SerializablePlaylist));
        using XmlReader xr = XmlReader.Create(fs);
        object? result = xmlSerializer.Deserialize(xr);
        if (result is SerializablePlaylist p)
        {
            playlist = p;
            return true;
        }
        else
        {
            playlist = null;
            return false;
        }
    }
}
