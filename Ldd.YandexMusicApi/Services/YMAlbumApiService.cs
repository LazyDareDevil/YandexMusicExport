using Ldd.YandexMusicApi.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ldd.YandexMusicApi.Services;

public static class YMAlbumApiService
{
    public static async Task<AlbumResponse?> TryGetAlbumData(this HttpClient client,
                                                             int albumId,
                                                             JsonSerializerOptions? options = null)
    {
        try
        {
            client.BaseAddress ??= YandexMusicApiService.ApiBaseAddress;
            HttpResponseMessage response = await client.GetAsync($"albums/{albumId}/with-tracks");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AlbumResponse>(options);
        }
        catch
        {
            return null;
        }
    }
}
