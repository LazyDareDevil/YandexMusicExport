namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class Album
{
    public string? Title { get; set; } = string.Empty;

    public int? Year { get; set; }

    public string? ReleaseDate { get; set; } = string.Empty;

    public string? Genre { get; set; } = string.Empty;

    public int? TrackCount { get; set; } = 0;

    public Artist[] Artists { get; set; } = [];

    public Label[] Labels { get; set; } = [];

    public string? CoverUri { get; set; } = string.Empty;

    public TrackPosition? TrackPosition { get; set; }
}
