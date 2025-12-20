namespace Ldd.MusicPlaylists.Domain;

public class Playlist
{
    public string Title { get; set; } = string.Empty;

    public Track[] Tracks { get; set; } = [];
}
