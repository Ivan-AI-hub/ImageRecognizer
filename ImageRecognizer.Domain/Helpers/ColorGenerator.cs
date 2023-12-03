using System.Collections;
using System.Drawing;

namespace ImageRecognizer.Domain.Helpers;

public static class ColorGenerator
{

    // RYB color space
    private static class RYB
    {
        private static readonly double[] White = { 1, 1, 1 };
        private static readonly double[] Red = { 1, 0, 0 };
        private static readonly double[] Yellow = { 1, 1, 0 };
        private static readonly double[] Blue = { 0.163, 0.373, 0.6 };
        private static readonly double[] Violet = { 0.5, 0, 0.5 };
        private static readonly double[] Green = { 0, 0.66, 0.2 };
        private static readonly double[] Orange = { 1, 0.5, 0 };
        private static readonly double[] Black = { 0.2, 0.094, 0.0 };

        public static double[] ToRgb(double r, double y, double b)
        {
            var rgb = new double[3];
            for (int i = 0; i < 3; i++)
            {
                rgb[i] = White[i] * (1.0 - r) * (1.0 - b) * (1.0 - y) +
                         Red[i] * r * (1.0 - b) * (1.0 - y) +
                         Blue[i] * (1.0 - r) * b * (1.0 - y) +
                         Violet[i] * r * b * (1.0 - y) +
                         Yellow[i] * (1.0 - r) * (1.0 - b) * y +
                         Orange[i] * r * (1.0 - b) * y +
                         Green[i] * (1.0 - r) * b * y +
                         Black[i] * r * b * y;
            }

            return rgb;
        }
    }

    private class Points : IEnumerable<double[]>
    {
        private readonly int pointsCount;
        private double[] picked;
        private int pickedCount;

        private readonly List<double[]> points = new List<double[]>();

        public Points(int count)
        {
            pointsCount = count;
        }

        private void Generate()
        {
            points.Clear();
            var numBase = (int)Math.Ceiling(Math.Pow(pointsCount, 1.0 / 3.0));
            var ceil = (int)Math.Pow(numBase, 3.0);
            for (int i = 0; i < ceil; i++)
            {
                points.Add(new[]
                {
                Math.Floor(i/(double)(numBase*numBase))/ (numBase - 1.0),
                Math.Floor(i/(double)numBase % numBase)/ (numBase - 1.0),
                Math.Floor((double)(i % numBase))/ (numBase - 1.0),
            });
            }
        }

        private double Distance(double[] p1)
        {
            double distance = 0;
            for (int i = 0; i < 3; i++)
            {
                distance += Math.Pow(p1[i] - picked[i], 2.0);
            }

            return distance;
        }

        private double[] Pick()
        {
            if (picked == null)
            {
                picked = points[0];
                points.RemoveAt(0);
                pickedCount = 1;
                return picked;
            }

            var d1 = Distance(points[0]);
            int i1 = 0, i2 = 0;
            foreach (var point in points)
            {
                var d2 = Distance(point);
                if (d1 < d2)
                {
                    i1 = i2;
                    d1 = d2;
                }

                i2 += 1;
            }

            var pick = points[i1];
            points.RemoveAt(i1);

            for (int i = 0; i < 3; i++)
            {
                picked[i] = (pickedCount * picked[i] + pick[i]) / (pickedCount + 1.0);
            }

            pickedCount += 1;
            return pick;
        }

        public IEnumerator<double[]> GetEnumerator()
        {
            Generate();
            for (int i = 0; i < pointsCount; i++)
            {
                yield return Pick();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static IEnumerable<Color> Generate(int alpha, int numOfColors)
    {
        var points = new Points(numOfColors);

        foreach (var point in points)
        {
            var rgb = RYB.ToRgb(point[0], point[1], point[2]);
            yield return Color.FromArgb(alpha,
                (int)Math.Floor(255 * rgb[0]),
                (int)Math.Floor(255 * rgb[1]),
                (int)Math.Floor(255 * rgb[2]));
        }
    }
}
