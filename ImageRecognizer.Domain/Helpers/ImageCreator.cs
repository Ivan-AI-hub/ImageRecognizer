using SkiaSharp;

namespace ImageRecognizer.Domain.Helpers;

public class ImageCreator
{
    public SKBitmap CreateTestImage(string labelsPath)
    {
        var directories = Directory.GetDirectories(labelsPath);
        var images = new List<SKBitmap>();

        foreach (var directory in directories)
        {
            var imageFiles = Directory.GetFiles(directory);

            var image = SKBitmap.Decode(imageFiles.First());
            images.Add(image);
        }

        var outputImage = CombineImages(images);

        return outputImage;
    }

    private SKBitmap CombineImages(List<SKBitmap> images)
    {
        var numRows = (int)Math.Ceiling(Math.Sqrt(images.Count));
        var numCols = numRows;

        var outputWidth = images[0].Width * numCols;
        var outputHeight = images[0].Height * numRows;

        var outputImage = new SKBitmap(outputWidth, outputHeight);

        using var canvas = new SKCanvas(outputImage);

        for (var i = 0; i < numRows; i++)
        {
            for (var j = 0; j < numCols; j++)
            {
                var imageIndex = i * numCols + j;

                if (imageIndex < images.Count)
                {
                    var x = j * images[0].Width;
                    var y = i * images[0].Height;

                    canvas.DrawBitmap(images[imageIndex], new SKRect(x, y, x + images[0].Width, y + images[0].Height));
                }
            }
        }

        return outputImage;
    }
}
