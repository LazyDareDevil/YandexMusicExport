namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class PlaylistResponse
{
    public PlaylistResult Result { get; set; } = new();
}
