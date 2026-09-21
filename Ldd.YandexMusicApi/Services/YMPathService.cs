using System.Diagnostics.CodeAnalysis;

namespace Ldd.YandexMusicApi.Services;

public static class YMPathService
{
    // api playlist url style : https://api.music.yandex.net/users/user-id/playlists/playlist-id;
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "api.music.yandex.net";
    //uriParts[3] == "users";
    //uriParts[4] == "{user-id}"
    //uriParts[5] == "playlists"
    //uriParts[6] == "{playlist-id}"
    public static bool TryParseApiStylePlaylistPath(string link, out int userId, out int playlistId)
    {
        string[] urlParts = link.Split('/', '?');
        userId = -1;
        playlistId = -1;
        return urlParts.Length >= 7
            && urlParts[3].Equals("users", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(urlParts[4], out userId)
            && urlParts[5].Equals("playlists", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(urlParts[6], out playlistId);
    }

    //web app playlist url style : https://music.yandex.ru/playlists/lk.guid OR https://music.yandex.ru/playlists/guid
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "music.yandex.ru";
    //uriParts[3] == "playlists";
    //uriParts[4] == "lk.{some-guid}"; OR uriParts[4] = "{some-guid}";
    public static bool TryParseWebAppStylePlaylistPath(string link, [MaybeNullWhen(false)] out string playlistUuid)
    {
        string[] urlParts = link.Split('/', '?');
        playlistUuid = null;
        if (urlParts.Length < 5
            || !urlParts[3].Equals("playlists", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string id = urlParts[4];
        playlistUuid = id;
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

    // api playlist url style : https://api.music.yandex.net/album/album-id;
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "api.music.yandex.net";
    //uriParts[3] == "album";
    //uriParts[4] == "{album-id}"
    internal static bool TryParseApiStyleAlbumPath(string[] pathParts, [MaybeNullWhen(false)] out int albumId)
    {
        albumId = -1;
        return pathParts.Length >= 5
            && pathParts[3].Equals("album", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(pathParts[4], out albumId);
    }

    // api playlist url style : https://music.yandex.ru/album/album-id;
    //uriParts[0] == "https:";
    //uriParts[1] == "";
    //uriParts[2] == "music.yandex.ru";
    //uriParts[3] == "album";
    //uriParts[4] == "{album-id}"
    internal static bool TryParseWebAppStyleAlbumPath(string[] pathParts, [MaybeNullWhen(false)] out int albumId)
    {
        albumId = -1;
        return pathParts.Length >= 5
            && pathParts[3].Equals("album", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(pathParts[4], out albumId);
    }

    internal static bool TryGetCoverPathLink(string coverUrl, [MaybeNullWhen(false)] out string link)
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
}
