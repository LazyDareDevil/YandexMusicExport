using Ldd.MusicFilesMetadata.Parameters;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Ldd.MusicFilesMetadata;

public static class MusicFiles
{
    public static async Task<bool> TryReplaceFileMetadata(string filePath, MusicFileAttributes attributes)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        StorageFile file;
        MusicProperties musicProperties;
        try
        {
            file = await StorageFile.GetFileFromPathAsync(filePath);
            musicProperties = await file.Properties.GetMusicPropertiesAsync();
        }
        catch
        {
            return false;
        }

        if (attributes.Title is not null)
        {
            musicProperties.Title = attributes.Title;
        }

        if (attributes.AlbumArtists is not null)
        {
            musicProperties.AlbumArtist = string.Join("; ", attributes.AlbumArtists);
        }

        if (attributes.Artists is not null)
        {
            musicProperties.Artist = string.Join("; ", attributes.Artists);
        }

        if (attributes.Album is not null)
        {
            musicProperties.Album = attributes.Album;
        }

        if (attributes.Genres is not null)
        {
            musicProperties.Genre.Clear();
            foreach (string genre in attributes.Genres)
            {
                musicProperties.Genre.Add(genre);
            }
        }

        if (attributes.TrackNumber > 0)
        {
            musicProperties.TrackNumber = attributes.TrackNumber;
        }
        else if (musicProperties.TrackNumber != 0)
        {
            musicProperties.TrackNumber = 0;
        }

        musicProperties.Year = attributes.Year;

        try
        {
            await file.Properties.SavePropertiesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}