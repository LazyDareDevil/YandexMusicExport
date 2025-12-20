namespace Ldd.MusicPlaylists.Domain;

public class Track
{
    public string Title { get; set; } = string.Empty;

    public string CoverUri { get; set; } = string.Empty;

    public string CoverFilePath { get; set; } = string.Empty;

    public string[] Artists { get; set; } = [];

    public Album[] Albums { get; set; } = [];

    public int Volume { get; set; }

    public int Index { get; set; }
}
