namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class TrackResult
{
    public Track track { get; set; } = new();
}

#pragma warning restore IDE1006 // Naming Styles