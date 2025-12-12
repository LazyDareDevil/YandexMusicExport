using System.Xml.Serialization;

namespace YandexMusicExport.Serialization.Models;

[XmlRoot("Track")]
public class SerializableTrack
{
    public string Title { get; set; } = string.Empty;

    [XmlArrayItem("Artist")]
    public string[] Artists { get; set; } = [];

    [XmlArrayItem("Album")]
    public SerializableAlbum[] Albums { get; set; } = [];
}
