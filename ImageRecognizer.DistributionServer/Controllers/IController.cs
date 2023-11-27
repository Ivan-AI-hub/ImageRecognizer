using System.Net;

namespace ImageRecognizer.DistributionServer.Controllers;

public interface IController
{
    public string[] RawUrls { get; }
    public bool CheckUrl(string rawUrl);
    public Task ProcessRequestAsync(HttpListenerContext client);
}
