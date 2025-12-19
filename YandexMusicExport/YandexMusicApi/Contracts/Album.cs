namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class Album
{
    public string title { get; set; } = string.Empty;

    public int year { get; set; }

    public int trackCount { get; set; } = 0;

    public Artist[] artists { get; set; } = [];

    public string releaseDate { get; set; } = string.Empty;

    public string genre { get; set; } = string.Empty;

    public Label[] labels { get; set; } = [];

    public string? coverUri { get; set; } = string.Empty;

    public TrackPosition? trackPosition { get; set; }
}
#pragma warning restore IDE1006 // Naming Styles
