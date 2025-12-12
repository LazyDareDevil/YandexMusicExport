using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.YandexMusicApi;

public static class YMPlaylistPublicApiService
{
    public static string GetPlaylistDataRequestLink(int userId, int playlistId)
        => $"https://api.music.yandex.net/users/{userId}/playlists/{playlistId}";

    public static string GetPlaylistWebLink(string playlistUuid)
        => $"https://music.yandex.ru/playlists/{playlistUuid}";

    public static bool TryGetCoverPathLink(string coverUrl, [MaybeNullWhen(false)] out string link)
    {
        if (string.IsNullOrEmpty(coverUrl)
            || coverUrl.Length < 4)
        {
            link = null;
            return false;
        }

        link = $"https://{coverUrl[..-3]}/200x200";
        return true;
    }

    public static bool TryGetPlaylistApiDataFromWebAppData(this HttpClient client,
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

    public static async Task<PlaylistResponse?> GetPlaylist(this HttpClient client, string apiLink, JsonSerializerOptions? options = null)
    {
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, apiLink));
        return await response.Content.ReadFromJsonAsync<PlaylistResponse>(options);
    }
}
