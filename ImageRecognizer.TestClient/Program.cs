using System.Net.Http.Json;
using ImageRecognizer.Domain;

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

using HttpClient client = new HttpClient();
int port = 5100;
await client.PostAsJsonAsync($"http://127.0.0.1:{port}/", request);
client.Dispose();