using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ImageRecognizer.Domain;
using ImageRecognizer.Domain.Extencions;

int serverPort = 5100;
int port = GetRandomUnusedPort();

var server = new HttpListener();
server.Prefixes.Add($"http://127.0.0.1:{port}/");
server.Start();

var request = new LogicUnitRequest()
{
    Port = port,
};

using HttpClient startClient = new HttpClient();
await startClient.PostAsJsonAsync($"http://127.0.0.1:{serverPort}/logicUnit", request);
startClient.Dispose();

Console.WriteLine($"Server is listener at the https://localhost:{port}");

while (true)
{
    var client = await server.GetContextAsync();

    Console.WriteLine("+1 request");

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

        Console.WriteLine($"Image prediction stopped with Base64:\n{pictureResponse.HeatMapBase64}");
    }
    finally
    {
        Console.WriteLine("-1 request");
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