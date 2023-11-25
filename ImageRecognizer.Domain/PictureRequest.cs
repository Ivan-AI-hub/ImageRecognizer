namespace ImageRecognizer.Domain;

public class PictureRequest
{
    public Picture Picture { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
}
