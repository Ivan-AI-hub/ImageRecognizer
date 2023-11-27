
using ImageRecognizer.Domain;
using ImageRecognizer.Domain.Extencions;
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
        Console.WriteLine("+1 logic unit");
        LogicUnitStorage.AddUnit($"http://127.0.0.1:{content.Port}/");
    }
}
