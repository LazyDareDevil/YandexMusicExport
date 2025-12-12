using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml;
using System.Xml.Serialization;
using YandexMusicExport.Serialization;
using YandexMusicExport.Serialization.Enum;
using YandexMusicExport.Serialization.Models;
using YandexMusicExport.YandexMusicApi;
using YandexMusicExport.YandexMusicApi.Contracts;

namespace YandexMusicExport;

internal static class Program
{
    private static readonly Encoding _dataEncdoding = Encoding.Unicode;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private static void Main(string[] args)
    {
        string directory = AppContext.BaseDirectory;

        try
        {
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

            // Разделение исходного URL-адреса по символу "/"
            string[] uriParts = uriRaw.Split('/', '?');

            HttpClient client = new();
            bool correct = YMPlaylistPathService.TryParseApiStylePath(uriParts, out int userId, out int playlistId);
            if (!correct)
            {
                if (YMPlaylistPathService.TryParseWebAppStylePath(uriParts, out string? playlistUuid))
                {
                    correct = YMPlaylistPublicApiService.TryGetPlaylistApiDataFromWebAppData(client, uriRaw, playlistUuid, out userId, out playlistId);
                }
            }

            if (!correct)
            {
                return;
            }

            // Формирование URL-адреса для запроса к серверу Яндекс Музыки
            string uri = $"https://api.music.yandex.net/users/{userId}/playlists/{playlistId}";

            // Отправка запроса по URL-адресу и получение ответа в формате JSON
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            // Добавлен энкодинг на все, чтобы не было проблем с символами
            Task<PlaylistResponse?> responseDataTask =
                response.Content.ReadFromJsonAsync<PlaylistResponse>(_jsonOptions);
            responseDataTask.Wait();
            PlaylistResponse? responseData = responseDataTask.Result;
            if (responseDataTask.Exception is not null ||
                responseData is null ||
                string.IsNullOrEmpty(responseData.Result.PlaylistUuid))
            {
                CannotAccessDataError(responseDataTask.Exception);
                Console.ReadLine();
                return;
            }

            string playlisPublicLink = YMPlaylistPathService.GetPlaylistPublicLink(responseData.Result.PlaylistUuid);
            string playlistTitle = responseData.Result.Title;
            string outputFileName = $"{playlistTitle}_{DateTime.Now:yyyy-MM-dd}";
            string outputFilePath = string.Empty;
            if (export == ExportType.PlainText)
            {
                outputFileName = DefaultFileExport(outputFileName, directory, responseData);
            }
            else
            {
                SerializablePlaylist playlist = ModelMappingService.CreateSerilzableProject(responseData.Result);
                playlist.PlaylistPublicLink = playlisPublicLink;
                if (export == ExportType.Json)
                {
                    outputFilePath = JsonFileExport(outputFileName, directory, playlist);
                }

                if (export == ExportType.Xml)
                {
                    outputFilePath = XmlFileExport(outputFileName, directory, playlist);
                }
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка! Выбран некорректный тип сохранения плейлиста!");
                Console.ResetColor();
                return;
            }

            // Вывод информации
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Готово!\n");
            Console.WriteLine($"Список треков сохранен рядом с файлом программы (файл {outputFilePath}).\n");
            Console.ResetColor();
            Process.Start(new ProcessStartInfo(outputFilePath) { UseShellExecute = true });
        }
        catch (JsonException e)
        {
            CannotAccessDataError(e);
        }

        catch (IndexOutOfRangeException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Вероятно, некорректная ссылка. Проверьте, чтобы она была вида " +
                              "https://music.yandex.ru/users/USERNAME/playlists/PLAYLIST_ID или https://music.yandex.ru/playlists/PLAYLIST_UUID. Попробуйте еще раз. Если ничего не работает, напишите мне https://t.me/aleqsanbr.");
            Console.ResetColor();
            Console.WriteLine("\nДополнительная информация:");
            Console.WriteLine(e);
        }

        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка! Проверьте правильность ссылки и попробуйте еще раз. " +
                              "Также учтите, что из-за большого количества запросов может последовать временный " +
                              "бан от Яндекса. В таком случае попробуйте с другого устройства или на сайте " +
                              "https://files.u-pov.ru/programs/YandexMusicExport" + ". " +
                              "Если ничего не работает, напишите мне https://t.me/aleqsanbr.");
            Console.ResetColor();
            Console.WriteLine("\nДополнительная информация:");
            Console.WriteLine(e);

        }

        Console.WriteLine("\nДля закрытия нажмите любую клавишу...");
        Console.ReadKey();
    }

    private static void CannotAccessDataError(Exception? ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка! Несуществующий плейлист или временный бан от Яндекса. Проверьте ссылку " +
                          "и попробуйте еще раз через некоторое время или на другом устройстве. Также есть сайт " +
                          "https://files.u-pov.ru/programs/YandexMusicExport" +
                          ", но велика вероятность, что там " +
                          "вообще ничего не будет работать :) \n" +
                          "Если вообще ничего не помогает, напишите мне https://t.me/aleqsanbr.");
        Console.ResetColor();
        Console.WriteLine("\nДополнительная информация:");
        Console.WriteLine(ex);
    }

    private static string DefaultFileExport(string outputFileName, string directory, PlaylistResponse responseData)
    {
        string outputFilePath = Path.Combine(directory, $"{outputFileName}.txt");
        Console.WriteLine("Список треков:");
        using (StreamWriter textFile = new(outputFilePath))
        {
            foreach (Track track in responseData.Result.Tracks.Select(t => t.Track))
            {
                string line = string.Format("{0} - {1}", string.Join(", ", track.Artists.Select(a => a.Name)).TrimEnd(',', ' '), track.Title);
                Console.WriteLine("\t" + line);
                textFile.WriteLine(line);
            }
        }

        return outputFilePath;
    }

    private static string JsonFileExport(string outputFileName, string directory, SerializablePlaylist playlist)
    {
        string outputFilePath = Path.Combine(directory, $"{outputFileName}.json");
        using FileStream fs = new(outputFilePath, new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
        });
        JsonSerializer.Serialize(fs, playlist, _jsonOptions);
        return outputFilePath;
    }

    private static string XmlFileExport(string outputFileName, string directory, SerializablePlaylist playlist)
    {
        string outputFilePath = Path.Combine(directory, $"{outputFileName}.xml");
        using StreamWriter fs = new(outputFilePath, new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
        });
        SerializeXml(fs, playlist);
        return outputFilePath;
    }

    private static void SerializeXml<T>(StreamWriter stream, T data)
        where T : class
    {
        XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
        XmlSerializer xmlSerializer = new(typeof(T));
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = _dataEncdoding
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        xmlSerializer.Serialize(writer, data, emptyNamespaces);
    }
}
