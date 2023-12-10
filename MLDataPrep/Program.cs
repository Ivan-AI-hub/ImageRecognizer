using ImageRecognizer.Domain.Helpers;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

var path = "D:\\For laba\\1.Ready laboratory\\4 course\\Course\\PSP\\ImageRecognizer\\ImageRecognizer.Domain\\Labels\\";

ImageCreator imageCreator = new ImageCreator();
using var image = imageCreator.CreateTestImage(path);

//image.Save($"{path}test.jpg");
//foreach (string directory in Directory.EnumerateDirectories(path))
//{
//    Console.WriteLine($"Start {directory} directory");
//    var files = Directory.EnumerateFiles(directory).ToArray();
//    Console.WriteLine($"{files.Count()} files");
//    foreach (string file in files)
//    {
//        var name = Path.GetFileNameWithoutExtension(file);
//        var ex = Path.GetExtension(file);
//        using var image = new Bitmap(Image.FromFile(file));

//        var sizeScale = 2;
//        int k = 1;
//        for (int i = 0; i < image.Width-10; i += image.Width/sizeScale)
//        {
//            for (int j = 0; j < image.Height-10; j += image.Height/sizeScale)
//            {
//                using var cropped = ImageHandler.CropImage(image, new Point(i, j), new Size(image.Width / sizeScale, image.Height / sizeScale));
//                cropped.Save(Path.Combine(directory, $"{name}_{k}{ex}"));

//                k++;
//            }
//        }
//    }

//    Console.WriteLine("End directory");
//}