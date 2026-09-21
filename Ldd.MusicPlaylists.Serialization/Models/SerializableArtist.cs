using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ldd.MusicPlaylists.Serialization.Models;

[Serializable]
[XmlRoot("Artist")]
public class SerializableArtist
{
    [XmlAttribute]
    public int Id { get; set; }

    [XmlAttribute]
    public string Name { get; set; } = string.Empty;
}
