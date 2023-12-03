using Google.Protobuf;
using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace ImageRecognizer.Domain.Helpers;

public class FruitHelper : IDisposable
{
    private MD5 _hacher;
    private Color[] _colors;

    public FruitHelper(int alpha, int labelsCount)
    {
        _hacher = MD5.Create();
        _colors = ColorGenerator.Generate(alpha, labelsCount + 2).Skip(2).ToArray();
    }

    public void Dispose()
    {
        _hacher.Dispose();
    }

    public Color GetColor(string label)
    {
        int someHash = CalculateHash(label);
        return _colors[someHash % _colors.Length];
    }

    private int CalculateHash(string message)
    {
        var hashed = _hacher.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Math.Abs(BitConverter.ToInt32(hashed, 0));
    }
}
