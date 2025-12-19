namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class PlaylistResult
{
    public string? PlaylistUuid { get; set; } = string.Empty;

    public int Uid { get; set; }

    public int Kind { get; set; }

    public string? Title { get; set; } = string.Empty;

    public int TrackCount { get; set; }

    public TrackResult[] Tracks { get; set; } = [];
}
