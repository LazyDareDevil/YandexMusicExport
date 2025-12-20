using System.Net.Http.Json;
using System.Text.Json;
using YandexMusicExport.YandexMusicApi.Responses;

namespace YandexMusicExport.YandexMusicApi;

public static class YMTrackDownloadPublicApiService
{
    // https://api.music.yandex.net/tracks/{trackid}/download-info
    // response as TrackDownloadResponse downloadInfo
    // url = downloadInfo[i].downloadInfoUrl
    // https://url
    // response as TrackDownloadSource downloadSource
    // https://{downloadSource.Host}{downloadSource.Path}
    // auth required

    public static async Task<PlaylistResponse?> TryGetTrackDownloadData(this HttpClient client, int trackId, JsonSerializerOptions? options = null)
    {
        // Формирование URL-адреса для запроса к серверу Яндекс Музыки
        string uri = YMPlaylistPathService.GetTrackDownloadInfoLink(trackId);
        try
        {
            // Отправка запроса по URL-адресу и получение ответа в формате JSON
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            return await response.Content.ReadFromJsonAsync<PlaylistResponse>(options);
        }
        catch
        {
            return null;
        }
    }
}
