using System.Reflection.Emit;

namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class Album
{
    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string Genre { get; set; } = string.Empty;

    public int TrackCount { get; set; } = 0;

    public string CoverUri { get; set; } = string.Empty;

    public Artist[] Artists { get; set; } = [];

    public Label[] Labels { get; set; } = [];
}
