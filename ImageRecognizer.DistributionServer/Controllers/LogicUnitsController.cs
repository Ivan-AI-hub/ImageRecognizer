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
            else if(request.HttpMethod == HttpMethod.Delete.Method)
            {
                RemoveLogicUnit(client);
            }
        }

        client.Response.Close();
    }

    private void RemoveLogicUnit(HttpListenerContext client)
    {
        var url = client.Request.RemoteEndPoint.Address;

        LogicUnitStorage.RemoveUnit(LogicUnitStorage.FreeUnitsUrls.First(x => x.Contains(url.ToString())));

        Console.WriteLine($"-1 logic unit {url}");
        Console.WriteLine($"logicUnits {LogicUnitStorage.FreeUnitsCount}");
    }

    private void AddLogicUnit(HttpListenerContext client)
    {
        var content = client.Request.GetData<LogicUnitRequest>();
        var url = $"http://localhost:{content.Port}/";

        LogicUnitStorage.AddUnit(url);

        Console.WriteLine($"+1 logic unit {url}");
        Console.WriteLine($"logicUnits {LogicUnitStorage.FreeUnitsCount}");
    }
}
