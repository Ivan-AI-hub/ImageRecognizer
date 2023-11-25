namespace ImageRecognizer.Domain;

public class Picture
{
    public string Base64Content { get; set; }
    public string Name { get; set; }
    public Picture(string base64Content, string name)
    {
        Base64Content = base64Content;
        Name = name;
    }
}
