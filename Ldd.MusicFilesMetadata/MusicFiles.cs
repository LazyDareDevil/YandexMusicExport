using Ldd.MusicFilesMetadata.Parameters;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Ldd.MusicFilesMetadata;

public static class MusicFiles
{
    public static bool TryRenameFile(string filePath, Regex findPattern, [MaybeNullWhen(false)] out string newFilePath)
    {
        newFilePath = null;
        if (!File.Exists(filePath))
        {
            return false;
        }

        Regex r = new(@"(\d)+\w-\w(<fileName>)\w\[audiovk\.com\]");
        string? direstoryPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(direstoryPath))
        {
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string fileExtension = Path.GetExtension(filePath);
        string newFileName =  findPattern.Replace(fileName, "");
        newFilePath = Path.Combine(direstoryPath, $"{newFileName}{fileExtension}");
        try
        {
            File.Move(filePath, newFilePath);
            return true;
        }
        catch
        {
            newFilePath = null;
            return false;
        }
    }

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

        if (attributes.TrackNumber.HasValue)
        {
            musicProperties.TrackNumber = attributes.TrackNumber.Value;
        }

        if (attributes.Year.HasValue)
        {
            musicProperties.Year = attributes.Year.Value;
        }

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