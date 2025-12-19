using System.Text.Json;
using YandexMusicExport.Serialization.Models;

namespace YandexMusicExport.Serialization;

public static class JsonSerialization
{
    public static bool TryJsonFileExport(string outputFilePath, SerializablePlaylist playlist, JsonSerializerOptions? options = null)
    {
        try
        {
            using FileStream fs = new(outputFilePath, new FileStreamOptions()
            {
                Mode = FileMode.OpenOrCreate,
                Access = FileAccess.Write,
            });
            JsonSerializer.Serialize(fs, playlist, options);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
