namespace Ldd.MusicFilesMetadata.Parameters;

public sealed class MusicFileAttributes
{
    public uint? TrackNumber { get; set; }

    public string? Title { get; set; }

    public string[]? Artists { get; set; }

    public string[]? AlbumArtists { get; set; }

    public string[]? Genres { get; set; }

    public string? Album { get; set; }

    public uint? Year { get; set; }
}
