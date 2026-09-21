using System.Text;

namespace Ldd.YandexMusicApi.Services;

public static class YMPublicApiLinkService
{
    public static string GetPlaylistPublicLink(string playlistUuid)
        => $"https://music.yandex.ru/playlists/{playlistUuid}";
}
