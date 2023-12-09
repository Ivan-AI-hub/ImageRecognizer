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

AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);

int port = 5100;

var request = new LogicUnitRequest()
{
    Port = port,
};

try
{
    using HttpClient startClient = new HttpClient();
    var res = await startClient.PostAsJsonAsync($"http://172.20.0.2:5100/logicUnit", request);
    startClient.Dispose();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
    return;
}

var localAddress = GetLocalIPAddress();
var server = new HttpListener();
server.Prefixes.Add($"http://{localAddress}:{port}/");
server.Start();

Console.WriteLine($"Unit is listener at the http://{localAddress}:{port}");

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

string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}