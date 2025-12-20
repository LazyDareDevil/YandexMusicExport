using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Serialization;
using YandexMusicExport.YandexMusicApi.Contracts;
using YandexMusicExport.YandexMusicApi.Responses;

namespace YandexMusicExport.YandexMusicApi;

public static class YMTrackDownloadPublicApiService
{
    public static async Task<TrackDownloadInfo[]> TryGetTrackDownloadInfoData(this HttpClient client, int trackId, JsonSerializerOptions? options = null)
    {
        try
        {
            // Формирование URL-адреса для запроса к серверу Яндекс Музыки
            string uri = YMPublicApiLinkService.GetTrackDownloadInfoLink(trackId);
            // Отправка запроса по URL-адресу и получение ответа в формате JSON
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            TrackDownloadResponse? downloadResponse = await response.Content.ReadFromJsonAsync<TrackDownloadResponse>(options);
            if (downloadResponse is null)
            {
                return [];
            }

            return downloadResponse.trackDownloadTypes;
        }
        catch
        {
            return [];
        }
    }

    public static async Task<TrackDownloadSourceResponse?> GetDownloadSourceData(this HttpClient client, TrackDownloadInfo downloadInfo)
    {
        try
        {
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
            string uri = GetDownloadDataLink(downloadSource);
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            return await response.Content.ReadAsStreamAsync();
        }
        catch
        {
            return null;
        }
    }

    private static string GetDownloadDataLink(TrackDownloadSourceResponse trackDownloadSource)
        => $"https://{trackDownloadSource.Host}{trackDownloadSource.Path}";
}
