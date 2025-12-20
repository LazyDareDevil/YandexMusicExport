namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class TrackDownloadInfo
{
    public string codec { get; set; } = string.Empty;

    public string downloadInfoUrl { get; set; } = string.Empty;

    public int bitrateInKbps { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles