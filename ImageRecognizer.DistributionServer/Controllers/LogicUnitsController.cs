using ImageRecognizer.Domain.Extencions;
using ImageRecognizer.Domain.Requests;
using System.Net;

namespace ImageRecognizer.DistributionServer.Controllers;

public class LogicUnitsController : ControllerBase
{
    public override string[] RawUrls { get; init; }

    public LogicUnitsController()
    {
        RawUrls = new string[] { "/logicUnit" };
    }

    public override async Task ProcessRequestAsync(HttpListenerContext client)
    {
        var request = client.Request;
        if (request.RawUrl == "/logicUnit")
        {
            if (request.HttpMethod == HttpMethod.Post.Method)
            {
                AddLogicUnit(client);
            }
        }

        client.Response.Close();
    }

    private void AddLogicUnit(HttpListenerContext client)
    {
        var content = client.Request.GetData<LogicUnitRequest>();
        var url = $"http://{client.Request.RemoteEndPoint.Address}:{content.Port}/";

        Console.WriteLine($"+1 logic unit {url}");

        LogicUnitStorage.AddUnit(url);
    }
}
