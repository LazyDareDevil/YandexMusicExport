using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Xml;
using System.Xml.Serialization;

namespace YandexMusicExport;

internal static class Program
{
    private static void Main()
    {
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

            // Извлечение имени владельца и типа плейлиста из списка uriRaw
            string owner = uriParts[4];
            string playlistUuIdPart = uriParts[4];
            string kinds = "";

            HttpClient client = new();

            if (uriParts.Length > 6)
            {
                owner = uriParts[6];
            }

            string playlistUuid = playlistUuIdPart;
            if (playlistUuIdPart.Length > 3 
                && playlistUuIdPart.StartsWith("lk."))
            {
                playlistUuid = playlistUuIdPart[3..];
            }

            if (Guid.TryParse(playlistUuid, out Guid parsedGuid))
            {
                HttpResponseMessage playlistHtml = client.Send(new HttpRequestMessage(HttpMethod.Get, uriRaw));
                Task<string> readTask = playlistHtml.Content.ReadAsStringAsync();
                readTask.Wait();
                Regex playlistDataReg = new("\"uuid\":\"" + playlistUuIdPart + "\".+\"uid\":(?<useruid>[0-9]+).+\"kind\":(?<playlistkind2>[0-9]+)");
                string data = readTask.Result;
                data = data.Replace("\n", "");
                data = data.Replace("\t", "");
                data = data.Replace(" ", "");
                foreach (Match match in playlistDataReg.Matches(data))
                {
                    if (match.Groups.TryGetValue("useruid", out Group? group))
                    {
                        owner = group.Value;
                    }

                    if (match.Groups.TryGetValue("playlistkind2", out Group? group2))
                    {
                        kinds = group2.Value;
                    }
                }
            }

            // Формирование URL-адреса для запроса к серверу Яндекс Музыки
            string uri = $"https://api.music.yandex.net/users/{owner}/playlists/{kinds}";

            // Отправка запроса по URL-адресу и получение ответа в формате JSON
            HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
            Task<PlaylistResponse?> responseDataTask = response.Content.ReadFromJsonAsync<PlaylistResponse>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            responseDataTask.Wait();
            PlaylistResponse? responseData = responseDataTask.Result;
            if (responseDataTask.Exception is not null || 
                responseData is null ||
                string.IsNullOrEmpty(responseData.Result.PlaylistUuid))
            {
                CannonAcessDataError(responseDataTask.Exception);
                Console.ReadLine();
                return;
            }

            string playlistTitle = responseData.Result.Title;
            string outputFileName = $"{playlistTitle}_{DateTime.Now:yyyy-MM-dd}";
            string outputFilePath;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Название плейлиста: {playlistTitle}");
            switch (export)
            {
                case ExportType.Json:
                    outputFilePath = $"{outputFileName}.json";
                    SerializablePlaylist jsonplaylist = CreateSerilzableProject(responseData.Result);
                    using (FileStream fs = new (outputFilePath, new FileStreamOptions()
                    {
                        Mode = FileMode.OpenOrCreate,
                        Access = FileAccess.ReadWrite,
                    }))
                    {
                        JsonSerializer.Serialize(fs, jsonplaylist, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = false,
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                        });
                    }
                    break;
                case ExportType.Xml:
                    outputFilePath = $"{outputFileName}.xml";
                    SerializablePlaylist xmlplaylist = CreateSerilzableProject(responseData.Result);
                    using (StreamWriter fs = new(outputFilePath, new FileStreamOptions()
                    {
                        Mode = FileMode.OpenOrCreate,
                        Access = FileAccess.ReadWrite,
                    }))
                    {
                        SerializeXml(fs, xmlplaylist);
                    }
                     
                    break;
                default:
                    outputFilePath = $"{outputFileName}.txt";
                    Console.WriteLine("Список треков:");
                    using (StreamWriter textFile = new(outputFilePath))
                    {
                        textFile.WriteLine(GetPlaylistPublicLink(responseData.Result));
                        foreach (Track track in responseData.Result.Tracks.Select(t => t.Track))
                        {
                            string line = string.Format("{0} - {1}", string.Join(", ", track.Artists.Select(a => a.Name)).TrimEnd(',', ' '), track.Title);
                            Console.WriteLine("\t" + line);
                            textFile.WriteLine(line);
                        }
                    }

                    break;
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
            CannonAcessDataError(e);
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

    private static void CannonAcessDataError(Exception? ex)
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

    private static SerializablePlaylist CreateSerilzableProject(PlaylistResult playlist)
        => new()
        {
            Title = playlist.Title,
            TrackCount = playlist.TrackCount,
            PlaylistPublicLink = GetPlaylistPublicLink(playlist),
            Tracks = [.. playlist.Tracks.Select(t => t.Track).Select( t => new SerializableTrack(){
                Title = t.Title,
                Artists = [..t.Artists.Select(a => a.Name)],
                Albums = [..t.Albums.Select(a => new SerializableAlbum(){
                    Title = a.Title,
                    Year = a.Year,
                    TrackCount = a.TrackCount,
                    Artists = [..a.Artists.Select(a => a.Name)],
                })]
            })]
        };

    private static string GetPlaylistPublicLink(PlaylistResult playlist) => $"https://music.yandex.ru/playlists/{playlist.PlaylistUuid}";

    private static void SerializeXml<T>(StreamWriter stream, T data)
        where T : class
    {
        XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
        XmlSerializer xmlSerializer = new(typeof(T));
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8
        };
        
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        xmlSerializer.Serialize(writer, data, emptyNamespaces);
    }
}

enum ExportType
{
    PlainText = 1,
    Json = 2,
    Xml = 3
}

[Serializable]
public class PlaylistResponse
{
    public PlaylistResult Result { get; set; } = new();
}

[Serializable]
public class PlaylistResult
{
    public string PlaylistUuid { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int TrackCount { get; set; } = 0;

    public TrackResult[] Tracks { get; set; } = [];
}

[Serializable]
public class TrackResult
{
    public Track Track { get; set; } = new();
}

[Serializable]
public class Track
{
    public string Title { get; set; } = string.Empty;

    public Artist[] Artists { get; set; } = [];

    public Album[] Albums { get; set; } = [];
}

[Serializable]
public class Artist
{
    public string Name { get; set; } = string.Empty;
}

[Serializable]
public class Album 
{
    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }

    public int TrackCount { get; set; } = 0;

    public Artist[] Artists { get; set; } = [];
}

[Serializable]
[XmlRoot("Playlist")]
public class SerializablePlaylist
{
    [XmlAttribute]
    public string PlaylistPublicLink { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;

    [XmlAttribute]
    public int TrackCount { get; set; } = 0;

    [XmlArrayItem("Track")]
    public SerializableTrack[] Tracks { get; set; } = [];
}

[XmlRoot("Track")]
public class SerializableTrack
{
    public string Title { get; set; } = string.Empty;

    [XmlArrayItem("Artist")]
    public string[] Artists { get; set; } = [];

    [XmlArrayItem("Album")]
    public SerializableAlbum[] Albums { get; set; } = [];
}

[XmlRoot("Album")]
public class SerializableAlbum
{
    public string Title { get; set; } = string.Empty;

    [XmlAttribute]
    public int Year { get; set; }

    [XmlAttribute]
    public int TrackCount { get; set; } = 0;

    [XmlArrayItem("Artist")]
    public string[] Artists { get; set; } = [];
}
