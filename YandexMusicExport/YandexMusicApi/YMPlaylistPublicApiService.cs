using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.YandexMusicApi;

public static class YMPlaylistPublicApiService
{
    public static bool TryParsePlaylistApiData(this HttpClient client, string link, out int userId, out int playlistId)
    {
        // Разделение исходного URL-адреса по символу "/"
        string[] uriParts = link.Split('/', '?');
        bool correct = YMPlaylistPathService.TryParseApiStylePath(uriParts, out userId, out playlistId);
        if (!correct)
        {
            if (YMPlaylistPathService.TryParseWebAppStylePath(uriParts, out string? playlistUuid))
            {
                return TryGetPlaylistApiDataFromWebAppData(client, link, playlistUuid, out userId, out playlistId);
            }
        }

        return correct;
    }

    private static bool TryGetCoverPathLink(string coverUrl, [MaybeNullWhen(false)] out string link)
    {
        if (string.IsNullOrEmpty(coverUrl)
            || coverUrl.Length < 4)
        {
            link = null;
            return false;
        }

        string croppedUrl = coverUrl[..^2];
        link = $"https://{croppedUrl}200x200";
        return true;
    }

    public static async Task<Stream?> GetCoverImageDataStrem(this HttpClient client, string coverUri)
    {
        if (!TryGetCoverPathLink(coverUri, out string? link))
        {
            return null;
        }

        try
        {
            HttpResponseMessage message = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, link));
            return message.Content.ReadAsStream();
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetPlaylistApiDataFromWebAppData(this HttpClient client,
                                                           string playlistLink,
                                                           string playlistUuid,
                                                           [MaybeNullWhen(false)] out int userId,
                                                           [MaybeNullWhen(false)] out int playlistId)
    {
        userId = -1;
        playlistId = -1;
        // В ответ придет полный HTML страницы, там же содержится информация о плейлисте - 
        // USER_ID и PLAYLIST_ID
        HttpResponseMessage playlistHtml = client.Send(new HttpRequestMessage(HttpMethod.Get, playlistLink));
        Task<string> readTask = playlistHtml.Content.ReadAsStringAsync();
        readTask.Wait();
        // понять, как сделать проверку без привязки порядка userid - playlistkind
        Regex playlistDataReg = new("\"uuid\":\"" + playlistUuid + "\".+\"uid\":(?<useruid>[0-9]+).+\"kind\":(?<playlistkind>[0-9]+)");
        string data = readTask.Result;
        data = data.Replace("\n", "");
        data = data.Replace("\t", "");
        data = data.Replace(" ", "");
        bool userFound = false;
        bool playlistFound = false;
        foreach (Match match in playlistDataReg.Matches(data))
        {
            userFound = match.Groups.TryGetValue("useruid", out Group? group)
                        && int.TryParse(group.Value, out userId);
            playlistFound = match.Groups.TryGetValue("playlistkind", out Group? group2)
                        && int.TryParse(group2.Value, out playlistId);
        }

        return userFound && playlistFound;
    }

    public static async Task<PlaylistResponse?> TryGetPlayliistData(this HttpClient client, int userId, int playlistId, JsonSerializerOptions? options = null)
    {
        // Формирование URL-адреса для запроса к серверу Яндекс Музыки
        string uri = YMPlaylistPathService.GetPlaylistDataRequestLink(userId, playlistId);
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
