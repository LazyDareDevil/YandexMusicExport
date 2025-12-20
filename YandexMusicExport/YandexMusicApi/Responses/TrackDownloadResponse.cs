using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.YandexMusicApi.Responses;

#pragma warning disable IDE1006 // Naming Styles
[Serializable]
public class TrackDownloadResponse
{
    public TrackDownloadInfo[] trackDownloadTypes { get; set; } = [];
}

#pragma warning restore IDE1006