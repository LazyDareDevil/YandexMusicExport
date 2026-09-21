using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Ldd.MusicPlaylists.Serialization.Models;

[XmlRoot("Track")]
public class SerializableTrack
{
    [XmlAttribute]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [XmlIgnore]
    [JsonIgnore]
    public string CoverUri { get; set; } = string.Empty;

    [XmlIgnore]
    [JsonIgnore]
    public string CoverFilePath { get; set; } = string.Empty;

    [XmlArrayItem("Artist")]
    public SerializableArtist[] Artists { get; set; } = [];

    [XmlArrayItem("Album")]
    public SerializableAlbum[] Albums { get; set; } = [];

    [XmlAttribute]
    public int Volume { get; set; }

    [XmlAttribute]
    public int Index { get; set; }
}
