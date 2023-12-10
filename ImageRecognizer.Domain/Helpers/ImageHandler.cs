using ImageRecognizer_LogicUnit;
using SkiaSharp;

namespace ImageRecognizer.Domain.Helpers;

public static class ImageHandler
{
    public static SKBitmap ConvertBase64ToBitmap(string base64String)
    {
        Console.WriteLine($"{DateTime.Now} b64 {base64String.Length}");

        var bytes = Convert.FromBase64String(base64String);
        using MemoryStream memoryStream = new(bytes)
        {
            Position = 0
        };

        return SKBitmap.Decode(memoryStream);
    }

    public static SKBitmap CropImage(SKBitmap image, SKPointI point, SKSizeI size)
    {
        SKRectI rectI = new(point.X, point.Y, point.X + size.Width, point.Y + size.Height);
        SKBitmap subset = new(image.Info);

        image.ExtractSubset(subset, rectI);

        return subset;
    }

    public static byte[] ConvertImageToByteArray(SKBitmap image)
    {
        using var stream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        return stream.ToArray();
    }

    public static SKBitmap PredictImage(SKBitmap image, int width, int height)
    {
        var outputImage = image.Copy();

        var widthReminder = image.Width % width;
        var heightReminder = image.Height % height;

        Console.WriteLine("Start prediction");
        Console.WriteLine($"Image ({image.Width}X{image.Height})");
        Console.WriteLine($"Window ({width}X{height})");
        Console.WriteLine($"Reminder ({widthReminder}X{heightReminder})");

        using FruitHelper helper = new(100, 28);

        for (int i = 0; i < image.Width; i += width)
        {
            for (int j = 0; j < image.Height; j += height)
            {
                PredictProcess(image, outputImage, i, j, width, height, helper);
            }
        }

        return outputImage;
    }

    private static void PredictProcess(SKBitmap image, SKBitmap outputImage, int pointX, int pointY, int windowWidth, int windowHeight, FruitHelper helper, int iteration = 1)
    {
        if (iteration == 3 || iteration > 1 && (windowWidth < 100 || windowHeight < 100))
        {
            return;
        }
        var point = new SKPointI(pointX, pointY);
        var size = new SKSizeI(windowWidth, windowHeight);

        using var cropped = CropImage(image, point, size);

        var croppedBytes = ConvertImageToByteArray(cropped);

        FruitClassificator.ModelInput sampleData = new FruitClassificator.ModelInput()
        {
            ImageSource = croppedBytes,
        };

        var output = FruitClassificator.Predict(sampleData);

        if (output.PredictedLabel == "Fons")
        {
            return;
        }
        else if (output.Score.Max() > 0.6)
        {
            using var canvas = new SKCanvas(outputImage);
            using var paint = new SKPaint { Color = helper.GetColor(output.PredictedLabel) };

            var rect = new SKRect(point.X, point.Y, point.X + size.Width, point.Y + size.Height);

            canvas.DrawRect(rect, paint);

            var labels = FruitClassificator.GetSortedScoresWithLabels(output).Take(1);
            var text = string.Join("\n", labels.Select(l => $"{string.Join("", l.Key.Take(5))}: {l.Value.ToString("0.00")}"));

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center,
                TextSize = 20.0f,
                Typeface = SKTypeface.CreateDefault(),
            };

            float xText = rect.MidX;
            float yText = rect.MidY + textPaint.TextSize / 2;
            canvas.DrawText(text, xText, yText, textPaint);
        }
        else
        {
            for (int i = 0; i < windowWidth; i += windowWidth / 2)
            {
                for (int j = 0; j < windowHeight; j += windowHeight / 2)
                {
                    PredictProcess(image, outputImage, pointX + i, pointY + j, windowWidth / 2, windowHeight / 2, helper, iteration + 1);
                }
            }
        }
    }
}