using System.Diagnostics;
using System.Net.Http.Json;
using ImageRecognizer.Domain;
using ImageRecognizer.Domain.Requests;

string filePatch;
int windowHeight;
int windowWidth;

Console.WriteLine("Select file");

var files = Directory.GetFiles("Data\\");
for (int i = 1; i < files.Length + 1 ; i++)
{
    Console.WriteLine($"{i}. {files[i-1]}");
}

var fileIndex = int.Parse(Console.ReadLine()) - 1;

filePatch = files[fileIndex];
Console.Write("Window height: ");
windowHeight = int.Parse(Console.ReadLine());

Console.Write("Window width: ");
windowWidth = int.Parse(Console.ReadLine());

using var file = new FileStream(filePatch, FileMode.Open);

Console.WriteLine("Request created");
var stream = new MemoryStream();
await file.CopyToAsync(stream);

var picture = new Picture(Convert.ToBase64String(stream.ToArray()), file.Name);

var request = new PictureRequest()
{
    Picture = picture,
    WindowHeight = windowHeight,
    WindowWidth = windowWidth,
};
Stopwatch stopwatch = new Stopwatch();

while (true)
{
    using HttpClient client = new HttpClient();
    int port = 5100;
    stopwatch.Start();
    var res = await client.PostAsJsonAsync($"http://127.0.0.1:{port}/predict", request);
    stopwatch.Stop();
    client.Dispose();
    Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}");
    Console.WriteLine("press anything to resend...");
    Console.ReadLine();

    stopwatch.Restart();
}
