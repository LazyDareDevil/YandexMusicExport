namespace YandexMusicExport.YandexMusicApi;

public static class YMPublicApiLinkService
{
    public static string GetPlaylistDataRequestLink(int userId, int playlistId)
        => $"https://api.music.yandex.net/users/{userId}/playlists/{playlistId}";

    public static string GetPlaylistPublicLink(string playlistUuid)
        => $"https://music.yandex.ru/playlists/{playlistUuid}";

    public static string GetAlbumWithTracksLink(int albumId)
        => $"https://api.music.yandex.net/albums/{albumId}/with-tracks";

    public static string GetTrackDownloadInfoLink(int trackId)
        => $"https://api.music.yandex.net/tracks/{trackId}/download-info";
}
