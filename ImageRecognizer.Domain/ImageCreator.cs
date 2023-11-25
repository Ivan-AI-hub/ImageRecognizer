using System.Drawing;

namespace ImageRecognizer.Domain;

public class ImageCreator
{
    public Bitmap CreateTestImage(string labelsPath)
    {
        var directories = Directory.GetDirectories(labelsPath);
        var images = new List<Bitmap>();

        foreach (var directory in directories)
        {
            var imageFiles = Directory.GetFiles(directory);

            var image = new Bitmap(imageFiles.First());
            images.Add(image);
        }

        var outputImage = CombineImages(images);

        return outputImage;
    }

    private Bitmap CombineImages(List<Bitmap> images)
    {
        var numRows = (int)Math.Ceiling(Math.Sqrt(images.Count));
        var numCols = numRows;

        var outputWidth = images[0].Width * numCols;
        var outputHeight = images[0].Height * numRows;

        var outputImage = new Bitmap(outputWidth, outputHeight);

        using var g = Graphics.FromImage(outputImage);

        for (var i = 0; i < numRows; i++)
        {
            for (var j = 0; j < numCols; j++)
            {
                var imageIndex = i * numCols + j;

                if (imageIndex < images.Count)
                {
                    var x = j * images[0].Width;
                    var y = i * images[0].Height;

                    g.DrawImage(images[imageIndex], x, y);
                }
            }
        }

        return outputImage;
    }
}
