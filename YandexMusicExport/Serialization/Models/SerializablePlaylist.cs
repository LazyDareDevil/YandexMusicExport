using System.Xml;
using System.Xml.Serialization;

namespace YandexMusicExport.Serialization.Models;

[Serializable]
[XmlRoot("Playlist")]
public class SerializablePlaylist
{
    [XmlAttribute]
    public string PlaylistPublicLink { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [XmlAttribute]
    public int TrackCount { get; set; } = 0;

    [XmlArrayItem("Track")]
    public SerializableTrack[] Tracks { get; set; } = [];
}
