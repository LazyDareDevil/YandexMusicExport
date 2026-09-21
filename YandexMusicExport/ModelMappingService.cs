using Ldd.MusicPlaylists.Serialization.Models;
using Ldd.YandexMusicApi;
using Ldd.YandexMusicApi.Contracts;

namespace YandexMusicExport;

public static class ModelMappingService
{
    public static SerializablePlaylist CreateSerilzableProject(Playlist playlist)
        => new()
        {
            Title = playlist.title,
            //PlaylistPublicLink = YandexMusicApiService.GetPlaylistPublicLink(playlist.playlistUuid),
            Uid = playlist.uid,
            Kind = playlist.kind,
            TrackCount = playlist.trackCount,
            Tracks = [.. playlist.tracks.Select(t => t.track).Select( t => new SerializableTrack() {
                Id = t.id,
                Title = t.title,
                Artists = [..t.artists.Select(a => new SerializableArtist()
                {
                    Id = a.id,
                    Name = a.name
                })],
                CoverUri = t.coverUri,
                Volume = t.albums.FirstOrDefault()?.trackPosition?.volume ?? 0,
                Index = t.albums.FirstOrDefault()?.trackPosition?.index ?? 0,
                Albums = [..t.albums.Select(a => new SerializableAlbum() {
                    Id = a.id,
                    Title = a.title,
                    Artists = [..a.artists.Select(a => new SerializableArtist()
                    {
                        Id = a.id,
                        Name = a.name
                    })],
                    Year = a.year,
                    TrackCount = a.trackCount,
                    ReleaseDate = a.releaseDate,
                    Genre = a.genre,
                    Labels = [..a.labels.Select(a => a.name)]
                })]
            })]
        };
}
