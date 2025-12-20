using System.Xml.Serialization;

namespace YandexMusicExport.YandexMusicApi.Responses;

[XmlRoot("download-info")]
public class TrackDownloadSource
{
    [XmlElement("host")]
    public string Host { get; set; } = string.Empty;

    [XmlElement("path")]
    public string Path { get; set; } = string.Empty;

    [XmlElement("ts")]
    public int TS { get; set; }

    [XmlElement("s")]
    public string S { get; set; } = string.Empty;
}
