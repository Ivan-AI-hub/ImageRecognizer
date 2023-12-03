using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ImageRecognizer.Domain.Extencions;
using ImageRecognizer.Domain.Helpers;
using ImageRecognizer.Domain.Requests;
using ImageRecognizer.Domain.Responses;

string serverAddress = "127.0.0.1";

Console.WriteLine("Write server address, or press enter");
string? wrServer = Console.ReadLine();

if (!string.IsNullOrEmpty(wrServer))
{
    serverAddress = wrServer;
}

string serverUrl = $"http://{serverAddress}:5100";

int port = GetRandomUnusedPort();

var server = new HttpListener();
server.Prefixes.Add($"http://127.0.0.1:{port}/");
server.Start();

var request = new LogicUnitRequest()
{
    Port = port,
};

try
{
    using HttpClient startClient = new HttpClient();
    await startClient.PostAsJsonAsync($"{serverUrl}/logicUnit", request);
    startClient.Dispose();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
}

Console.WriteLine($"Unit is listener at the http://127.0.0.1:{port}");

while (true)
{
    var client = await server.GetContextAsync();

    ThreadPool.QueueUserWorkItem((status) => HandleClientsAsync(client));
}

void HandleClientsAsync(HttpListenerContext client)
{
    try
    {
        var request = client.Request;
        if (request.HttpMethod != HttpMethod.Post.Method)
        {
            return;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Console.WriteLine("Image prediction start");
        var content = request.GetData<PictureRequest>();

        using var image = ImageHandler.ConvertBase64ToBitmap(content.Picture.Base64Content);
        using var outputImage = ImageHandler.PredictImage(image, content.WindowWidth, content.WindowHeight);

        var pictureResponse = new PictureResponse()
        {
            HeatMapBase64 = Convert.ToBase64String(ImageHandler.ConvertImageToByteArray(outputImage))
        };

        HttpListenerResponse response = client.Response;

        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pictureResponse));

        response.ContentLength64 = buffer.Length;
        Stream responseStream = response.OutputStream;
        responseStream.Write(buffer, 0, buffer.Length);
        responseStream.Close();

        stopwatch.Stop();
        Console.WriteLine($"Image prediction stop, elapsed {stopwatch.ElapsedMilliseconds}");
    }
    finally
    {
        client.Response.Close();
    }
}

int GetRandomUnusedPort()
{
    var listener = new TcpListener(IPAddress.Any, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}