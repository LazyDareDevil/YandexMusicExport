using System.Text;

namespace Ldd.YandexMusicApi.Services;

internal static class ApiExtensions
{
    internal static async Task<string> GetRawResponse(this HttpResponseMessage response, Encoding? encoding = null)
    {
        Stream content = await response.Content.ReadAsStreamAsync();
        byte[] data = new byte[content.Length];
        content.ReadExactly(data);
        content.Position = 0;
        return (encoding ?? Encoding.UTF8).GetString(data);
    }

    internal static async Task SaveRawResponse(this HttpResponseMessage response, string filePath)
    {
        Stream content = await response.Content.ReadAsStreamAsync();
        using FileStream fs = new(filePath, FileMode.Truncate, FileAccess.Write);
        content.CopyTo(fs);
        content.Position = 0;
        fs.Flush();
    }
}
