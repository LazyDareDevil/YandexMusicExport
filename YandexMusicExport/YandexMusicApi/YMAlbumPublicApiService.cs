using System.Net.Http.Json;
using System.Text.Json;
using YandexMusicExport.YandexMusicApi.Responses;

namespace YandexMusicExport.YandexMusicApi;

public static class YMAlbumPublicApiService
{
    public static async Task<AlbumResponse?> TryGetAlbumData(this HttpClient client, int albumId, JsonSerializerOptions? options = null)
    {
        // Формирование URL-адреса для запроса к серверу Яндекс Музыки
        string uri = YMPlaylistPathService.GetAlbumWithTracksLink(albumId);
        try
        {
            // Отправка запроса по URL-адресу и получение ответа в формате JSON
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            return await response.Content.ReadFromJsonAsync<AlbumResponse>(options);
        }
        catch
        {
            return null;
        }
    }
}
