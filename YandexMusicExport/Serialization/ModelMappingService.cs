using YandexMusicExport.Serialization.Models;
using YandexMusicExport.YandexMusicApi;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport.Serialization;

public static class ModelMappingService
{
    public static SerializablePlaylist CreateSerilzableProject(PlaylistResult playlist)
        => new()
        {
            Title = playlist.Title,
            PlaylistPublicLink = YMPlaylistPathService.GetPlaylistPublicLink(playlist.PlaylistUuid),
            TrackCount = playlist.TrackCount,
            Tracks = [.. playlist.Tracks.Select(t => t.Track).Select( t => new SerializableTrack(){
                Title = t.Title,
                Artists = [..t.Artists.Select(a => a.Name)],
                Albums = [..t.Albums.Select(a => new SerializableAlbum(){
                    Title = a.Title,
                    Year = a.Year,
                    ReleaseDate = a.ReleaseDate,
                    Type = a.Type,
                    Genre = a.Genre,
                    TrackCount = a.TrackCount,
                    CoverUri = a.CoverUri,
                    CoverImageFileName = a.CoverUri,
                    Artists = [..a.Artists.Select(a => a.Name)],
                    Labels = [..a.Labels.Select(a => a.Name)]
                })]
            })]
        };
}
