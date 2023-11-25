using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using ImageRecognizer.Domain;
using ImageRecognizer.Domain.Extencions;
using ImageRecognizer_Domain;

int port = 5000;

var server = new HttpListener();
server.Prefixes.Add($"http://127.0.0.1:{port}/");
server.Start();

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
        if (request.HttpMethod == HttpMethod.Get.Method)
        {
            //if (request.RawUrl == "/")
            //{
            //    client.SendFile("D:\\For laba\\1.Ready laboratory\\4 course\\1 sem\\PSP\\lab6\\client-server\\HttpServer\\wwwroot\\index.html", "index.html", "text/html");
            //}
            //else if (request.RawUrl == "/style.css")
            //{
            //    client.SendFile("D:\\For laba\\1.Ready laboratory\\4 course\\1 sem\\PSP\\lab6\\client-server\\HttpServer\\wwwroot\\style.css", "style.css", "text/css");
            //}
            //else if (request.RawUrl == "/client.js")
            //{
            //    client.SendFile("D:\\For laba\\1.Ready laboratory\\4 course\\1 sem\\PSP\\lab6\\client-server\\HttpServer\\wwwroot\\client.js", "client.js", "text/javascript");
            //}
        }
        else if (request.HttpMethod == HttpMethod.Post.Method)
        {
            var content = request.GetData<PictureRequest>();

            var image = ImageHandler.ConvertBase64ToBitmap(content.Picture.Base64Content);
            var outputImage = ImageHandler.PredictImage(image, content.WindowWidth, content.WindowHeight);

            //return value
            //var outputBase64 = Convert.ToBase64String(ImageHandler.ConvertImageToByteArray(outputImage));
            var folder = "D:\\For laba\\1.Ready laboratory\\4 course\\Курс\\PSP\\ImageRecognizer\\ImageRecognizer.Server\\Results\\";
            var fileName = $"{Guid.NewGuid()}.jpeg";
            outputImage.Save($"{folder}{fileName}");

            var a = 0;
        }

    }
    finally
    {
        Console.WriteLine("-1 request");
    }
}
