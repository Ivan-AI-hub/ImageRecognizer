using System.Net;
using System.Text.Json;

namespace ImageRecognizer.Domain.Extencions;

public static class HttpListenerRequestExtencions
{
    public static T? GetData<T>(this HttpListenerRequest request) where T : class
    {
        if (!request.HasEntityBody)
        {
            return null;
        }

        using Stream body = request.InputStream;
        using var reader = new StreamReader(body, request.ContentEncoding);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), options);
    }
}
