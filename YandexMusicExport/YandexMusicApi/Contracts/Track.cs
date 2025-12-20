namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class Track
{
    public int id { get; set; }

    public string title { get; set; } = string.Empty;

    public Artist[] artists { get; set; } = [];

    public Album[] albums { get; set; } = [];

    public string coverUri { get; set; } = string.Empty;
}

#pragma warning restore IDE1006 // Naming Styles