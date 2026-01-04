using Ldd.MusicPlaylists.Serialization.Models;
using YandexMusicExport.YandexMusicApi;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace Ldd.MusicPlaylistsConverter;

public static class ModelMappingService
{
    public static SerializablePlaylist CreateSerilzableProject(Playlist playlist)
        => new()
        {
            Title = playlist.title,
            PlaylistPublicLink = YMPublicApiLinkService.GetPlaylistPublicLink(playlist.playlistUuid),
            Uid = playlist.uid,
            Kind = playlist.kind,
            TrackCount = playlist.trackCount,
            Tracks = [.. playlist.tracks.Select(t => t.track).Select( t => new SerializableTrack() {
                Title = t.title,
                Artists = [..t.artists.Select(a => a.name)],
                CoverUri = t.coverUri,
                Volume = t.albums.FirstOrDefault()?.trackPosition?.volume ?? 0,
                Index = t.albums.FirstOrDefault()?.trackPosition?.index ?? 0,
                Albums = [..t.albums.Select(a => new SerializableAlbum() {
                    Title = a.title,
                    Artists = [..a.artists.Select(a => a.name)],
                    Year = a.year,
                    TrackCount = a.trackCount,
                    ReleaseDate = a.releaseDate,
                    Genre = a.genre,
                    Labels = [..a.labels.Select(a => a.name)]
                })]
            })]
        };
}
