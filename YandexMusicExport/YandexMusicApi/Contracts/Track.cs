namespace YandexMusicExport.YandexMusicApi.Contracts;

[Serializable]
public class Track
{
    public string Title { get; set; } = string.Empty;

    public Artist[] Artists { get; set; } = [];

    public Album[] Albums { get; set; } = [];
}
