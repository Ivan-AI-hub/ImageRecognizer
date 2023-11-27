using System.Net;

namespace ImageRecognizer.DistributionServer.Controllers;

public abstract class ControllerBase : IController
{
    public abstract string[] RawUrls { get; init; }

    public bool CheckUrl(string rawUrl)
    {
        if(RawUrls.Contains(rawUrl)) return true;
        return false;
    }
    public abstract Task ProcessRequestAsync(HttpListenerContext client);
}
