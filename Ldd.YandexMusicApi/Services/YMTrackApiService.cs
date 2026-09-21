using Ldd.YandexMusicApi.Contracts;
using Ldd.YandexMusicApi.Responses;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Serialization;

namespace Ldd.YandexMusicApi.Services;

public static class YMTrackApiService
{
    public static async Task<TrackDownloadResponse?> GetTrackDownloadData(this HttpClient client, int trackId, JsonSerializerOptions? options = null)
    {
        try
        {
            client.BaseAddress ??= YandexMusicApiService.ApiBaseAddress;
            HttpResponseMessage response = await client.GetAsync($"tracks/{trackId}/download-info");
            return await response.Content.ReadFromJsonAsync<TrackDownloadResponse>(options);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<TrackDownloadSourceResponse?> GetDownloadSourceData(this HttpClient client, TrackDownloadInfo downloadInfo)
    {
        try
        {
            client.BaseAddress = null;
            string uri = $"https://{downloadInfo.downloadInfoUrl}";
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            Stream responseData = await response.Content.ReadAsStreamAsync();
            XmlSerializer seriaslizer = new(typeof(TrackDownloadSourceResponse));
            object? deserialized = seriaslizer.Deserialize(responseData);
            if (deserialized is not TrackDownloadSourceResponse responseResult)
            {
                return null;
            }

            return responseResult;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<Stream?> GetTrackData(this HttpClient client, TrackDownloadSourceResponse downloadSource)
    {
        // TODO: auth required
        try
        {
            client.BaseAddress = null;
            string uri = $"https://{downloadSource.Host}{downloadSource.Path}";
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            return await response.Content.ReadAsStreamAsync();
        }
        catch
        {
            return null;
        }
    }
}
