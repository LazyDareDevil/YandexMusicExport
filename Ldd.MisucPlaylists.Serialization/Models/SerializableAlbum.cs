using System.Xml;
using System.Xml.Serialization;

namespace Ldd.MisucPlaylists.Serialization.Models;

[XmlRoot("Album")]
public class SerializableAlbum
{
    public string Title { get; set; } = string.Empty;

    [XmlAttribute]
    public int Year { get; set; }

    [XmlAttribute]
    public string ReleaseDate { get; set; } = string.Empty;

    [XmlAttribute]
    public string Genre { get; set; } = string.Empty;

    [XmlAttribute]
    public int TrackCount { get; set; } = 0;

    [XmlArrayItem("Artist")]
    public string[] Artists { get; set; } = [];

    [XmlArrayItem("Labels")]
    public string[] Labels { get; set; } = [];
}
