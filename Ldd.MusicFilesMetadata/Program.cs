//#define CHANGE_PROPERTIES
//#define RENAME_FILES
//#define MATCH_PLAYLIST
#define CHECK_ENCODING

#if RENAME_FILES
using System.Text.RegularExpressions;
#endif
#if CHANGE_PROPERTIES
using Windows.Storage;
using Windows.Storage.FileProperties;
#endif

using Microsoft.Win32;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Ldd.MusicFilesMetadata;

internal partial class Program
{
#if RENAME_FILES
    [GeneratedRegex("")]
    private static partial Regex RemoveNameItemPattern();
#endif

#if MATCH_PLAYLIST
    private const string trackNameSeparator = " - ";
#endif

    [STAThread]
    public static void Main(string[] args)
    {
#if CHECK_ENCODING
        OpenFileDialog dialog = new()
        {
            Multiselect = true,
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };
        bool? result = dialog.ShowDialog();
        if (!result.HasValue
            || !result.Value)
        {
            return;
        }

        Console.WriteLine(Path.GetFileNameWithoutExtension(dialog.FileNames[0]).Equals(Path.GetFileNameWithoutExtension(dialog.FileNames[1])));
        foreach (string fileName in dialog.FileNames)
        {
            PrintChars(Path.GetFileNameWithoutExtension(fileName), Encoding.Unicode);
        }
#endif

        Console.WriteLine("Введите путь до директории, в которой будет производиться модификация треков.");
        string? inputString = Console.ReadLine();
        if (string.IsNullOrEmpty(inputString)
            || !Directory.Exists(inputString))
        {
            Console.WriteLine("Введен несуществующий путь до плейлиста");
            return;
        }

        DirectoryInfo playlistDirectory = Directory.CreateDirectory(inputString);
        FileInfo[] directoryFiles = playlistDirectory.GetFiles();
#if RENAME_FILES
        Regex removePattern = RemoveNameItemPattern();
#endif
#if CHANGE_PROPERTIES
        StorageFile storageFile;
        MusicProperties musicProperties;
#endif
#if MATCH_PLAYLIST
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.WriteLine("Введите путь до файла со списком всех треков.");
        inputString = Console.ReadLine();
        if (string.IsNullOrEmpty(inputString)
            || !File.Exists(inputString))
        {
            Console.WriteLine("Введен несуществующий путь до файла");
            return;
        }

        SerializablePlaylist? playlist = null;
        string extention = Path.GetExtension(inputString);
        if (extention.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            XmlSerialization.TryLoadFromXml(inputString, out playlist);
        }

        if (playlist is null)
        {
            return;
        }

        Console.WriteLine("Неправильные именования файлов:");
        Dictionary<string, List<string>> existTrackFiles = [];
        foreach (FileInfo file in directoryFiles)
        {
            try
            {
                string[] nameSplit = file.Name[..^file.Extension.Length].Split(trackNameSeparator);
                string trackName = nameSplit[1];
                if (existTrackFiles.TryGetValue(trackName, out List<string>? value))
                {
                    value.Add(trackName);
                }
                else
                {
                    existTrackFiles.Add(trackName, [nameSplit[0]]);
                }
            }
            catch
            {
               Console.WriteLine(file.Name);
            }
        }

        List<SerializableTrack> notFoundArtists = [];
        List<SerializableTrack> notFoundTracks = [];
        foreach (SerializableTrack trackInfo in playlist.Tracks)
        {
            if (existTrackFiles.TryGetValue(trackInfo.Title, out List<string>? value))
            {
                string found = string.Empty;
                foreach (string art in value)
                {
                    if (trackInfo.Artists.Any(a => art.Contains(a)))
                    {
                        found = art;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(found))
                {
                    value.Remove(found);
                    continue;
                }

                notFoundArtists.Add(trackInfo);
                continue;
            }
            else
            {
                notFoundTracks.Add(trackInfo);
            }
        }

        Console.WriteLine($"\nНе найдено треков по названию: {notFoundTracks.Count}");
        foreach (SerializableTrack notFoundTrack in notFoundTracks.OrderBy(i => i.Artists[0]))
        {
            Console.WriteLine($"Не найдено название: {string.Join(", ", notFoundTrack.Artists)}{trackNameSeparator}{notFoundTrack.Title}");
        }

        Console.WriteLine($"\nНе найдено треков по автору: {notFoundArtists.Count}");
        foreach (SerializableTrack notFoundTrack in notFoundArtists.OrderBy(i => i.Artists[0]))
        {
            Console.WriteLine($"Не найден автор    : {string.Join(", ", notFoundTrack.Artists)}{trackNameSeparator}{notFoundTrack.Title}");
        }

#endif
        foreach (FileInfo file in directoryFiles)
        {
#if CHANGE_PROPERTIES
            try
            {
                Task<StorageFile> ft = StorageFile.GetFileFromPathAsync(file.FullName).AsTask();
                ft.Wait();
                storageFile = ft.Result;
                Task<MusicProperties> mt = storageFile.Properties.GetMusicPropertiesAsync().AsTask();
                mt.Wait();
                musicProperties = mt.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(file.FullName);
                Console.Error.WriteLine(ex.ToString());
            }
#endif
#if RENAME_FILES
            try
            {
            string newFileName = removePattern.Replace(file.Name, "");
            if (string.Equals(file.Name, newFileName))
            {
                continue;
            }

            file.MoveTo(Path.Combine(playlistPath, newFileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(file.FullName);
                Console.Error.WriteLine(ex.ToString());
            }
#endif
        }
    }

    static void PrintChars(string s, Encoding encoding)
    {
        Console.WriteLine(s);
        for (int i = 0; i < s.Length; i++)
        {
            Console.WriteLine("\ts[{0,1}] = '{1,-2}' {2,-50}", i, s[i], $"('\\u{(int)s[i]:x4}')");
        }

        //byte[] b = encoding.GetBytes(s);
        //Console.WriteLine("{0,-20} [{1,-50}]", encoding.EncodingName, string.Join(", ", b));
    }
}
