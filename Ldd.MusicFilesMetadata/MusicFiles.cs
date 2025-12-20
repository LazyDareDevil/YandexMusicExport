using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Ldd.MusicFilesMetadata;

public static class MusicFiles
{
    public static async Task GetMetadata(string filePath)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
    }
}
