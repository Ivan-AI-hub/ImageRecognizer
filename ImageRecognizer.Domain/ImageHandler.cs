using ImageRecognizer_Domain;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageRecognizer.Domain;

public static class ImageHandler
{
    public static Bitmap ConvertBase64ToBitmap(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);

        MemoryStream memoryStream = new MemoryStream(bytes);

        memoryStream.Position = 0;

        return new Bitmap(memoryStream);
    }
    public static Bitmap ResizeImage(Image image, int width, int height, bool preserveAspectRatio)
    {
        int drawWidth = width;
        int drawHeight = height;

        if (preserveAspectRatio)
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;
            float percentWidth = (float)width / (float)originalWidth;
            float percentHeight = (float)height / (float)originalHeight;
            float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
            drawWidth = (int)(originalWidth * percent);
            drawHeight = (int)(originalHeight * percent);
        }
        else
        {
            drawWidth = width;
            drawHeight = height;
        }

        var ResizeImage = new Rectangle(0, 0, drawWidth, drawHeight);
        var dest_Image = new Bitmap(drawWidth, drawHeight);

        dest_Image.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(dest_Image))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, ResizeImage, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return dest_Image;
    }

    public static Bitmap CropImage(Bitmap image, Point point, Size size)
    {
        Rectangle section = new Rectangle(point, size);

        Bitmap target = new Bitmap(size.Width, size.Height);
        using Graphics g = Graphics.FromImage(target);

        g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height),
            section,
            GraphicsUnit.Pixel);

        return target;
    }

    public static byte[] ConvertImageToByteArray(Bitmap image)
    {
        ImageConverter converter = new ImageConverter();
        return (byte[])converter.ConvertTo(image, typeof(byte[]));
    }

    public static Bitmap PredictImage(Bitmap image, int width, int height)
    {
        var outputImage = new Bitmap(image);

        var widthReminder = image.Width % width;
        var heightReminder = image.Height % height;

        Console.WriteLine("Start prediction");
        Console.WriteLine($"Image ({image.Width}X{image.Height})");
        Console.WriteLine($"Window ({width}X{height})");
        Console.WriteLine($"Reminder ({widthReminder}X{heightReminder})");

        for (int i = 0; i < image.Width; i += width)
        {
            for (int j = 0; j < image.Height; j += height)
            {
                PredictProcess(image, outputImage, i, j, width, height);
            }
        }

        return outputImage;
    }

    private static void PredictProcess(Bitmap image, Bitmap outputImage, int pointX, int pointY, int windowWidth, int windowHeight)
    {
        var point = new Point(pointX, pointY);
        var size = new Size(windowWidth, windowHeight);

        using var cropped = CropImage(image, point, size);

        var croppedBytes = ConvertImageToByteArray(cropped);

        FruitClassificator.ModelInput sampleData = new FruitClassificator.ModelInput()
        {
            ImageSource = croppedBytes,
        };

        var output = FruitClassificator.Predict(sampleData);

        if (output.Score.Max() > 0.9)
        {
            using Graphics gfx = Graphics.FromImage(outputImage);
            using SolidBrush brush = new SolidBrush(FruitHelper.GetColor(output.PredictedLabel));

            var rect = new RectangleF(point, size);

            gfx.FillRectangle(brush, rect);

            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gfx.DrawString(output.PredictedLabel, new Font("Tahoma", 14), Brushes.Black, rect);
        }
    }
}
