namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class AlbumResponse
{
    public Album result { get; set; } = new();
}

#pragma warning restore IDE1006 // Naming Styles