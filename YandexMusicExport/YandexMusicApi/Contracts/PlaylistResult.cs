namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class PlaylistResult
{
    public string PlaylistUuid { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int TrackCount { get; set; } = 0;

    public TrackResult[] Tracks { get; set; } = [];
}
