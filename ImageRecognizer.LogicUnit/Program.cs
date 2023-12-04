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

string selfAddress = "127.0.0.1";

Console.WriteLine("Write address for listening, or press enter for using default one");
string? wrServer = Console.ReadLine();

if (!string.IsNullOrEmpty(wrServer))
{
    selfAddress = wrServer;
}

int port = GetRandomUnusedPort();

var request = new LogicUnitRequest()
{
    Port = port,
};

try
{
    using HttpClient startClient = new HttpClient();
    var res = await startClient.PostAsJsonAsync($"http://26.152.192.178:5100/logicUnit", request);
    startClient.Dispose();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
    return;
}

var server = new HttpListener();
server.Prefixes.Add($"http://{selfAddress}:{port}/");
server.Start();

Console.WriteLine($"Unit is listener at the http://{selfAddress}:{port}");

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