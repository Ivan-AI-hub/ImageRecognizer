﻿using ImageRecognizer.Domain;
using System.Drawing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using ImageRecognizer.Domain.Extencions;

namespace ImageRecognizer.DistributionServer.Controllers;

public class DistributorController : ControllerBase
{
    private string[] _logicUnitsUrls => LogicUnitStorage.FreeUnitsUrls.ToArray();
    private int UnitsCount => LogicUnitStorage.FreeUnitsCount;

    public override string[] RawUrls { get; init; }

    public DistributorController()
    {
        RawUrls = new string[] { "/predict" };
    }

    public override async Task ProcessRequestAsync(HttpListenerContext client)
    {
        if (client.Request.RawUrl == "/predict")
        {
            if (client.Request.HttpMethod == HttpMethod.Post.Method)
            {
                await PredictImageAsync(client);
            }
        }

        client.Response.Close();
    }

    private async Task PredictImageAsync(HttpListenerContext client)
    {
        if (UnitsCount == 0)
        {
            client.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            return;
        }

        var content = client.Request.GetData<PictureRequest>();

        using var image = ImageHandler.ConvertBase64ToBitmap(content.Picture.Base64Content);
        //if (image.Width > maxWidth && image.Height > maxHeight)
        //{
        //    image = ImageHandler.ResizeImage(image, maxWidth, maxHeight, true);
        //}
        
        int widthReminder = image.Width % content.WindowWidth;
        var windows = (image.Width - widthReminder) / content.WindowWidth;

        var unitsAndWindows = CalculateWinowsByUnits(windows);

        int windowsReminder = windows - unitsAndWindows.windowsByUnits * unitsAndWindows.units;

        Console.WriteLine("Image processing start");
        Console.WriteLine($"LogicUnits: {UnitsCount}");
        Console.WriteLine($"used units: {unitsAndWindows.units}");
        Console.WriteLine($"windows by unit: {unitsAndWindows.windowsByUnits}");
        Console.WriteLine($"reminder: {windowsReminder}");

        var postTasks = SendDataToLogicUnits(image, unitsAndWindows.units, unitsAndWindows.windowsByUnits, 
                                    windowsReminder, content.WindowWidth, widthReminder, 
                                    content.WindowHeight, content.Picture.Name);

        HttpResponseMessage[] results = await Task.WhenAll(postTasks);

        using var outputImage = await ConcatHeatMapsAsync(results, image.Width, image.Height,
                                                          unitsAndWindows.units, unitsAndWindows.windowsByUnits, windowsReminder, 
                                                          content.WindowWidth, widthReminder);

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
    }

    private (int windowsByUnits, int units) CalculateWinowsByUnits(int windowsCount)
    { 
        int windowsByUnits = 0;
        int units = UnitsCount + 1;

        while (windowsByUnits == 0)
        {
            units--;

            if (units == 0)
            {
                units = 1;
                windowsByUnits = 1;
                break;
            }
            
            windowsByUnits = windowsCount / units;
            
        }

        return(windowsByUnits, units);
    }

    private IEnumerable<Task<HttpResponseMessage>> SendDataToLogicUnits(Bitmap image, int units, int windowsByUnit,
                                                               int windowsReminder, int windowWidth, int widthReminder,
                                                               int windowHeight, string pictureName)
    {
        var postTasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < units; i++)
        {
            var point = new Point(windowsByUnit * i * windowWidth, 0);
            Size size;
            if (i == units - 1)
            {
                size = new Size((windowsByUnit + windowsReminder) * windowWidth + widthReminder, image.Height);
            }
            else
            {
                size = new Size(windowsByUnit * windowWidth, image.Height);
            }
            Console.WriteLine($"{point}, {size}");
            var cropped = ImageHandler.CropImage(image, point, size);

            var croppedBytes = ImageHandler.ConvertImageToByteArray(cropped);

            cropped.Dispose();

            var picture = new Picture(Convert.ToBase64String(croppedBytes), pictureName);

            var pictureRequest = new PictureRequest()
            {
                Picture = picture,
                WindowHeight = windowHeight,
                WindowWidth = windowWidth,
            };

            HttpClient unitClient = new HttpClient();

            var task = unitClient.PostAsJsonAsync(_logicUnitsUrls[0], pictureRequest);
            LogicUnitStorage.AddWorkerUnit(_logicUnitsUrls[0]);

            postTasks.Add(task);
        }

        return postTasks;
    }

    private async Task<Bitmap> ConcatHeatMapsAsync(HttpResponseMessage[] logicUnitResponses, int imageWidth, int imageHeight, 
                                                    int units, int windowsByUnit,
                                                    int windowsReminder, int windowWidth, int widthReminder)
    {
        var heatMaps = logicUnitResponses.Select(async x => await x.Content.ReadFromJsonAsync<PictureResponse>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })).ToArray();

        IEnumerable<string> uris = logicUnitResponses.Select(x => x.RequestMessage?.RequestUri?.ToString() ?? string.Empty).Where(x => x != string.Empty);

        LogicUnitStorage.RemoveWorkerUnits(uris);

        var outputImage = new Bitmap(imageWidth, imageHeight);
        var gr = Graphics.FromImage(outputImage);
        for (int i = 0; i < units; i++)
        {
            var point = new Point(windowsByUnit * i * windowWidth, 0);
            Size size;
            if (i == units - 1)
            {
                size = new Size((windowsByUnit + windowsReminder) * windowWidth + widthReminder, imageHeight);
            }
            else
            {
                size = new Size(windowsByUnit * windowWidth, imageHeight);
            }

            var heatMap = await heatMaps[i];

            gr.DrawImage(ImageHandler.ConvertBase64ToBitmap(heatMap.HeatMapBase64), point.X, point.Y, size.Width, size.Height);
        }

        return outputImage;
    }
}