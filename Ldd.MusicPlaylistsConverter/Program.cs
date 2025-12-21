using Ldd.MusicPlaylists.Serialization;
using Ldd.MusicPlaylists.Serialization.Models;
using Ldd.MusicPlaylistsConverter;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using YandexMusicExport.YandexMusicApi;
using YandexMusicExport.YandexMusicApi.Contracts;
using YandexMusicExport.YandexMusicApi.Responses;

internal static class Program
{
    private static readonly Encoding _dataEncoding = Encoding.Unicode;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static void Main(string[] args)
    {
        string directory = AppContext.BaseDirectory;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.Write("=== Экспорт Яндекс Музыки ===");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" by https://t.me/aleqsanbr\n");
        Console.ResetColor();

        Console.WriteLine("!! ИНФОРМАЦИЯ !!");
        Console.WriteLine("Данная программа позволяет экспортировать любой плейлист Яндекс Музыки в текстовое " +
                            "представление ИМЯ ИСПОЛНИТЕЛЯ - НАЗВАНИЕ ТРЕКА.\n" +
                            "1. Скопируйте и вставьте ниже ссылку на плейлист. Обязательно проверьте, чтобы она была " +
                            "вида https://music.yandex.ru/users/USERNAME/playlists/PLAYLIST_ID" +
                            " или вида https://music.yandex.ru/playlists/lk.PLAYLIST_UUID или https://music.yandex.ru/playlists/PLAYLIST_UUID (данный вид в тестовом режиме)\n" +
                            "2. Если плейлист большой, может потребоваться некоторое время для обработки.\n" +
                            "3. Если ссылка корректная, но возникает ошибка, то, вероятно, сработал \"бан\" со " +
                            "стороны Яндекса. В таком случае попробуйте еще раз через некоторое время или на " +
                            "другом устройстве. Также есть сайт https://files.u-pov.ru/programs/YandexMusicExport, " +
                            "но там обычно вообще ничего не работает, так как все запросы пользователей посылаются с " +
                            "одного адреса.\n" +
                            "4. Вам необязательно вручную копировать весь вывод. Каждый раз автоматически создается " +
                            "файл НАЗВАНИЕ_ПЛЕЙЛИСТА.txt рядом с программой.\n" +
                            "5. Предложения, критика и прочее принимаются тута: https://t.me/aleqsanbr. В описании " +
                            "ссылка, подпишитесь на канал :)" +
                            "\n");
        string? uriRaw = null;
        while (string.IsNullOrEmpty(uriRaw))
        {
            Console.Write("Введите ссылку на плейлист Яндекс Музыки >>> ");
            uriRaw = Console.ReadLine();
        }

        ExportType export = ExportType.PlainText;
        Console.WriteLine("Выберите способ вывода данных: \n" +
                            "1. Список артистов - название трека (по умолчанию);\n" +
                            "2. json (расширенная информация) \n" +
                            "3. xml (расширенная информация) \n" +
                            "Нажмите Enter, чтобы выбрать способ по умолчанию");
        string? exportType = Console.ReadLine();
        if (int.TryParse(exportType, out int parsedInt) &&
            Enum.IsDefined(typeof(ExportType), parsedInt))
        {
            export = (ExportType)parsedInt;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Обработка...\n");
        Console.ResetColor();


        HttpClient client = new();
        if (!client.TryParsePlaylistApiData(uriRaw, out int userId, out int playlistId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Вероятно, некорректная ссылка. Проверьте, чтобы она была вида " +
                              "https://music.yandex.ru/users/USERNAME/playlists/PLAYLIST_ID или https://music.yandex.ru/playlists/PLAYLIST_UUID. Попробуйте еще раз. Если ничего не работает, напишите мне https://t.me/aleqsanbr.");
            Console.ResetColor();
            return;
        }

        Task<PlaylistResponse?> responseDataTask = client.TryGetPlaylistData(userId, playlistId, _jsonOptions);
        responseDataTask.Wait();
        if (responseDataTask.Result is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Проверьте правильность ссылки и попробуйте еще раз. " +
                              "Также учтите, что из-за большого количества запросов может последовать временный " +
                              "бан от Яндекса. В таком случае попробуйте с другого устройства или на сайте " +
                              "https://files.u-pov.ru/programs/YandexMusicExport" + ". " +
                              "Если ничего не работает, напишите мне https://t.me/aleqsanbr.");
            Console.ResetColor();
            return;
        }

        PlaylistResponse? responseData = responseDataTask.Result;
        string outputFilePath;
        bool serialized;
        SerializablePlaylist? playlist;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Начата обработка плейлиста '{responseData.result.title}'");
        Console.ResetColor();
        switch (export)
        {
            case ExportType.Json:
                {
                    playlist = ModelMappingService.CreateSerilzableProject(responseData.result);
                    outputFilePath = GetJsonFilePath(directory, responseData.result.title);
                    serialized = JsonSerialization.TryJsonFileExport(outputFilePath, playlist, _jsonOptions);
                    break;
                }
            case ExportType.Xml:
                {
                    playlist = ModelMappingService.CreateSerilzableProject(responseData.result);
                    outputFilePath = GetXmlFilePath(directory, responseData.result.title);
                    serialized = XmlSerialization.TryXmlFileExport(outputFilePath, playlist, _dataEncoding);
                    break;
                }
            default:
                {
                    outputFilePath = GetSimpleFilePath(directory, responseData.result.title);
                    serialized = TrySerializeSimpleText(outputFilePath, responseData);
                    break;
                }
        }

        if (string.IsNullOrEmpty(outputFilePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Выбран некорректный тип сохранения плейлиста!");
            Console.ResetColor();
            return;
        }

        if (!serialized)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Не удвлось сохранить данные в выбранном формате!");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Закончена обработка плейлиста '{responseData.result.title}'");
        Console.ResetColor();
        Console.WriteLine($"Список треков сохранен рядом с файлом программы (файл {outputFilePath}).\n");
        if (File.Exists(outputFilePath))
        {
            // открытие файла
            Process.Start(new ProcessStartInfo(outputFilePath) { UseShellExecute = true });
        }

        //if (playlist is not null)
        //{
        //    LoadCovers(client, directory, playlist).Wait();
        //}
    }

    private static string GetSimpleFilePath(string directory, string playlistName) => Path.Combine(directory, $"{playlistName}.txt");

    private static string GetXmlFilePath(string directory, string playlistName) => Path.Combine(directory, $"{playlistName}_{DateTime.Now:yyyy-MM-dd}.xml");

    private static string GetJsonFilePath(string directory, string playlistName) => Path.Combine(directory, $"{playlistName}_{DateTime.Now:yyyy-MM-dd}.json");

    public static bool TrySerializeSimpleText(string outputFilePath, PlaylistResponse responseData)
    {
        try
        {
            string publicLink = YMPublicApiLinkService.GetPlaylistPublicLink(responseData.result.playlistUuid);
            using StreamWriter textFile = new(outputFilePath);
            string lineText = $"Playlist '{responseData.result.title}' | {publicLink}";
            textFile.WriteLine(lineText);
            foreach (Track track in responseData.result.tracks.Select(t => t.track))
            {
                lineText = string.Format("{0} - {1}", string.Join(", ", track.artists.Select(a => a.name)).TrimEnd(',', ' '), track.title);
                textFile.WriteLine(lineText);
                Console.WriteLine(lineText);
            }

            textFile.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task LoadCovers(HttpClient client, string directory, SerializablePlaylist serializablePlaylist)
    {
        string dirPath = Path.Combine(directory, $"{serializablePlaylist.Title}-Covers");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        foreach (SerializableTrack track in serializablePlaylist.Tracks)
        {
            if (string.IsNullOrEmpty(track.CoverUri))
            {
                continue;
            }

            Stream? dataStream = await client.GetCoverImageDataStrem(track.CoverUri);
            if (dataStream is null)
            {
                continue;
            }

            string coverFilePath = Path.Combine(dirPath, $"{string.Join('_', track.Artists)}-{track.Title}.jpg");
            try
            {
                using FileStream fs = new(coverFilePath, FileMode.CreateNew, FileAccess.Write);
                dataStream.CopyTo(fs);
                fs.Flush();
                track.CoverFilePath = coverFilePath;
                dataStream.Dispose();
            }
            catch { }
        }
    }
}
