using Ldd.YandexMusicApi.Contracts;
using Ldd.YandexMusicApi.Responses;
using Ldd.YandexMusicApi.Services;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Ldd.YandexMusicApi;

public sealed class YandexMusicApiService
{
    private readonly Uri _baseAddress = new("https://api.music.yandex.net");
    private readonly CookieContainer _cookieContainer = new();
    private readonly HttpClientHandler _httpClientHandler;
    private readonly JsonSerializerOptions? _options;

    public void AddCookie(string name, string value) => _cookieContainer.Add(_baseAddress, new Cookie(name, value));

    public YandexMusicApiService(JsonSerializerOptions? options = null)
    {
        _httpClientHandler = new()
        {
            CookieContainer = _cookieContainer
        };
        _options = options ?? new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public HttpClient GetApiClient() => new(_httpClientHandler) { BaseAddress = YMPathService.ApiBaseAddress };

    public HttpClient GetClient() => new(_httpClientHandler);

    public async Task<Playlist?> GetPlaylist(int userId, int playlistId)
    {
        using HttpClient httpClient = GetApiClient();
        PlaylistResponse? response = await httpClient.TryGetPlaylistData(userId, playlistId, _options);
        return response?.result;
    }

    public async Task<Album?> TryGetAlbumData(int albumId)
    {
        using HttpClient httpClient = GetApiClient();
        AlbumResponse? response = await httpClient.TryGetAlbumData(albumId, _options);
        return response?.result;
    }

    public async Task<TrackDownloadInfo[]> TryGetTrackDownloadInfoData(int trackId)
    {
        using HttpClient client = GetApiClient();
        TrackDownloadResponse? downloadResponse = await client.GetTrackDownloadData(trackId, _options);
        return downloadResponse?.trackDownloadTypes ?? [];
    }
}
