namespace Ldd.MusicPlaylists.Domain;

public class Album
{
    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string Genre { get; set; } = string.Empty;

    public int TrackCount { get; set; } = 0;

    public string[] Artists { get; set; } = [];

    public string[] Labels { get; set; } = [];
}
