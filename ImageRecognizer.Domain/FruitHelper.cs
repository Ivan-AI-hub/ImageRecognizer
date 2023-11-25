using System.Drawing;

namespace ImageRecognizer.Domain;

public static class FruitHelper
{
    public static Color GetColor(string label)
    {
        switch(label)
        {
            case "Banana": return Color.FromArgb(100, 227, 207, 87);
            case "Carambula": return Color.FromArgb(100, Color.Yellow);
            case "Lemon": return Color.FromArgb(100, 255, 255, 159);
            case "Mango": return Color.FromArgb(100, 244, 187, 68);
            case "Tomato Heart": return Color.FromArgb(100, 255, 99, 71);
            case "Watermelon": return Color.FromArgb(100, Color.Green);
            default: return Color.Black;
        }
    }
}
