namespace YandexMusicExport.YandexMusicApi.Contracts;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class Volume
{
    public Track[] tracks { get; set; } = [];
}

#pragma warning restore IDE1006
