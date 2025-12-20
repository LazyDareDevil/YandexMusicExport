using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.YandexMusicApi.Responses;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class PlaylistResponse
{
    public Playlist result { get; set; } = new();
}

#pragma warning restore IDE1006 // Naming Styles