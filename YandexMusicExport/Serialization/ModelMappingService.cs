using YandexMusicExport.Serialization.Models;
using YandexMusicExport.YandexMusicApi;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.Serialization;

public static class ModelMappingService
{
    public static SerializablePlaylist CreateSerilzableProject(PlaylistResult playlist)
        => new()
        {
            Title = playlist.Title ?? string.Empty,
            PlaylistPublicLink = YMPlaylistPathService.GetPlaylistPublicLink(playlist.PlaylistUuid ?? string.Empty),
            Uid = playlist.Uid,
            Kind = playlist.Kind,
            TrackCount = playlist.TrackCount,
            Tracks = [.. playlist.Tracks.Select(t => t.Track).Select( t => new SerializableTrack() {
                Title = t.Title ?? string.Empty,
                Artists = [..t.Artists.Select(a => a.Name)],
                CoverUri = t.CoverUri ?? string.Empty,
                //Volume = t.Albums.FirstOrDefault()?.TrackPosition?.Volume ?? 0,
                //Index = t.Albums.FirstOrDefault()?.TrackPosition?.Index ?? 0,
                Albums = [..t.Albums.Select(a => new SerializableAlbum() {
                    Title = a.Title ?? string.Empty,
                    Year = a.Year ?? 0,
                    ReleaseDate = a.ReleaseDate ?? string.Empty,
                    Genre = a.Genre ?? string.Empty,
                    TrackCount = a.TrackCount ?? 0,
                    Artists = [..a.Artists.Select(a => a.Name)],
                    Labels = [..a.Labels.Select(a => a.Name)]
                })]
            })]
        };
}
