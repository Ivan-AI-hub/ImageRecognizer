using ImageRecognizer.DistributionServer.Controllers;
using System.Net;
AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
int port = 5100;

var controllers = new List<ControllerBase>() { new IndexController(), new DistributorController(), new LogicUnitsController() };

var server = new HttpListener();
server.Prefixes.Add($"http://*:{port}/");
server.Start();

Console.WriteLine($"Server is listener at the http://172.20.0.2:{port}/");
Console.WriteLine($"Server is listener at the http://localhost:{port}/");

while (true)
{
    var client = await server.GetContextAsync();

    Console.WriteLine($"Start working on request {client.Request.Url}");

    ThreadPool.QueueUserWorkItem((status) => HandleClientsAsync(client));
}

async void HandleClientsAsync(HttpListenerContext client)
{ 
    try
    {
        var request = client.Request;
        foreach(var controller in controllers)
        {
            if(controller.CheckUrl(request.RawUrl))
            {
                await controller.ProcessRequestAsync(client);
                break;
            }
        }    

    }
    finally
    {
        client.Response.Close();
    }
}
