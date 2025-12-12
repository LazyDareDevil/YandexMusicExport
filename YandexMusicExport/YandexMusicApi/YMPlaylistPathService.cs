using System.Diagnostics.CodeAnalysis;
using YandexMusicExport.Serialization.Models;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.YandexMusicApi;

internal static class YMPlaylistPathService
{
    // api playlist url style : https://api.music.yandex.net/users/user-id/playlists/playlist-id";
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "api.music.yandex.net";
    //uriParts[3] == "users";
    //uriParts[4] == "{user-id}"
    //uriParts[5] == "playlists"
    //uriParts[6] == "{playlist-id}"
    public static bool TryParseApiStylePath(string[] pathParts, [MaybeNullWhen(false)] out int userId, [MaybeNullWhen(false)] out int playlistId)
    {
        userId = -1;
        playlistId = -1;
        return pathParts.Length >= 7
            && pathParts[3].Equals("users", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(pathParts[4], out userId)
            && pathParts[5].Equals("playlists", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(pathParts[6], out playlistId);
    }

    //web app playlist url style : https://music.yandex.ru/playlists/lk.guid OR https://music.yandex.ru/playlists/guid
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "music.yandex.ru";
    //uriParts[3] == "playlists";
    //uriParts[4] == "lk.{some-guid}"; OR uriParts[4] = "{some-guid}";
    public static bool TryParseWebAppStylePath(string[] urlParts, [MaybeNullWhen(false)] out string playlistId)
    {
        playlistId = null;
        if (urlParts.Length < 5
            || !urlParts[3].Equals("playlists", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string id = urlParts[4];
        playlistId = id;
        // Если UUID плейлиста начинается с данной приставки, то ее нужно отрезать, чтобы проверить, что GUID валидный
        if (id.StartsWith("lk."))
        {
            if (id.Length < 4)
            {
                return false;
            }

            id = id[3..];
        }

        return Guid.TryParse(id, out _);
    }

    public static string GetPlaylistPublicLink(string playlistUuid) => $"https://music.yandex.ru/playlists/{playlistUuid}";
}
