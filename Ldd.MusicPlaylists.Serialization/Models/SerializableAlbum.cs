using System.Xml;
using System.Xml.Serialization;

namespace Ldd.MusicPlaylists.Serialization.Models;

[XmlRoot("Album")]
public class SerializableAlbum
{
    [XmlAttribute]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [XmlAttribute]
    public int Year { get; set; }

    [XmlAttribute]
    public string ReleaseDate { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public int TrackCount { get; set; } = 0;

    [XmlArrayItem("Artist")]
    public SerializableArtist[] Artists { get; set; } = [];

    [XmlArrayItem("Labels")]
    public string[] Labels { get; set; } = [];
}
