using System.Drawing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ImageRecognizer.Domain;
using ImageRecognizer.Domain.Extencions;

int port = 5100;
var logicUnitsPorts = new List<int>();
int maxWidth = 1000;
int maxHeight = 1000;

var server = new HttpListener();
server.Prefixes.Add($"http://127.0.0.1:{port}/");
server.Start();

Console.WriteLine($"Server is listener at the http://127.0.0.1:{port}");

while (true)
{
    var client = await server.GetContextAsync();

    Console.WriteLine("+1 request");

    ThreadPool.QueueUserWorkItem((status) => HandleClientsAsync(client));
}

async void HandleClientsAsync(HttpListenerContext client)
{
    try
    {
        var request = client.Request;
        if (request.HttpMethod == HttpMethod.Get.Method)
        {
            if (request.RawUrl == "/")
            {
                SendFile(client, "wwwroot\\index.html", "text/html");
            }
            else if (request.RawUrl == "/styles.css")
            {
                SendFile(client, "wwwroot\\styles.css", "text/css");
            }
            else if (request.RawUrl == "/script.js")
            {
                SendFile(client, "wwwroot\\script.js", "text/javascript");
            }
        }
        else if (request.HttpMethod == HttpMethod.Post.Method)
        {
            if (request.RawUrl == "/")
            {
                if(logicUnitsPorts.Count == 0)
                {
                    return;
                }

                var content = request.GetData<PictureRequest>();

                var image = ImageHandler.ConvertBase64ToBitmap(content.Picture.Base64Content);
                if (image.Width > maxWidth && image.Height > maxHeight)
                {
                    image = ImageHandler.ResizeImage(image, maxWidth, maxHeight, true);
                }

                int windowsByUnits = 0;
                int units = logicUnitsPorts.Count + 1;

                int widthReminder = image.Width % content.WindowWidth;
                var windows = (image.Width - widthReminder) / content.WindowWidth;
                while (windowsByUnits == 0)
                {
                    units--;

                    if (windows % units == 0)
                    {
                        windowsByUnits = windows / units;
                    }          
                }

                int reminder = windows % windowsByUnits;


                Console.WriteLine("Image processing start");
                Console.WriteLine($"LogicUnits: {logicUnitsPorts.Count}");
                Console.WriteLine($"used units: {units}");
                Console.WriteLine($"windows by unit: {windowsByUnits}");
                Console.WriteLine($"reminder: {reminder}");

                var postTasks = new List<Task<HttpResponseMessage>>();
                for(int i = 0; i < units; i++)
                {
                    var point = new Point(windowsByUnits * i * content.WindowWidth, 0);
                    Size size;
                    if(i == units-1)
                    {
                        size = new Size((windowsByUnits + reminder) * content.WindowWidth + widthReminder, image.Height);
                    }
                    else
                    {
                        size = new Size(windowsByUnits * content.WindowWidth, image.Height);
                    }
                    Console.WriteLine($"{point}, {size}");
                    var cropped = ImageHandler.CropImage(image, point, size);

                    var croppedBytes = ImageHandler.ConvertImageToByteArray(cropped);

                    cropped.Dispose();

                    var picture = new Picture(Convert.ToBase64String(croppedBytes), content.Picture.Name);

                    var pictureRequest = new PictureRequest()
                    {
                        Picture = picture,
                        WindowHeight = content.WindowHeight,
                        WindowWidth = content.WindowWidth,
                    };

                    HttpClient unitClient = new HttpClient();
                    var task = unitClient.PostAsJsonAsync($"http://127.0.0.1:{logicUnitsPorts[i]}/", pictureRequest);
                    postTasks.Add(task);
                }

                var results = await Task.WhenAll(postTasks);

                var heatMaps = results.Select(async x => await x.Content.ReadFromJsonAsync<PictureResponse>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })).ToArray();

                var outputImage = new Bitmap(image.Width, image.Height);
                var gr = Graphics.FromImage(outputImage);
                for(int i = 0; i < units; i++)
                {
                    var point = new Point(windowsByUnits * i * content.WindowWidth, 0);
                    Size size;
                    if (i == units - 1)
                    {
                        size = new Size((windowsByUnits + reminder) * content.WindowWidth + widthReminder, image.Height);
                    }
                    else
                    {
                        size = new Size(windowsByUnits * content.WindowWidth, image.Height);
                    }

                    var heatMap = await heatMaps[i];

                    gr.DrawImage(ImageHandler.ConvertBase64ToBitmap(heatMap.HeatMapBase64), point.X, point.Y, size.Width, size.Height);
                }

                image.Dispose();
                Console.WriteLine("Image processing stop");

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

                var a = 0;
            }
            else if (request.RawUrl == "/logicUnit")
            {
                var content = request.GetData<LogicUnitRequest>();
                Console.WriteLine("+1 logic unit");
                logicUnitsPorts.Add(content.Port);
            }
        }

    }
    finally
    {
        Console.WriteLine("-1 request");
        client.Response.Close();
    }
}

void SendFile(HttpListenerContext context, string path, string type)
{
    using HttpListenerResponse response = context.Response;
    response.Headers.Set("Content-Type", type);

    byte[] buf = File.ReadAllBytes(path);
    response.ContentLength64 = buf.Length;

    using Stream ros = response.OutputStream;
    ros.Write(buf, 0, buf.Length);
}
