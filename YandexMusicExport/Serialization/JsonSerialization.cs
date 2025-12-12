using System.Text.Json;
using YandexMusicExport.Serialization.Models;

namespace YandexMusicExport.Serialization;

public static class JsonSerialization
{
    public static string JsonFileExport(string outputFileName, string directory, SerializablePlaylist playlist, JsonSerializerOptions? options = null)
    {
        string outputFilePath = Path.Combine(directory, $"{outputFileName}.json");
        using FileStream fs = new(outputFilePath, new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
        });
        JsonSerializer.Serialize(fs, playlist, options);
        return outputFilePath;
    }
}
