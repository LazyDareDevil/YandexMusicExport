namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class Playlist
{
    public string playlistUuid { get; set; } = string.Empty;

    public string title { get; set; } = string.Empty;

    public int trackCount { get; set; } = 0;

    public TrackResult[] tracks { get; set; } = [];

    public int uid { get; set; }

    public int kind { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles