using ImageRecognizer.DistributionServer.Controllers;
using System.Net;

int port = 5100;

var controllers = new List<ControllerBase>() { new IndexController(), new DistributorController(), new LogicUnitsController() };

var server = new HttpListener();
server.Prefixes.Add($"http://26.152.192.178:{port}/");
server.Start();

Console.WriteLine($"Server is listener at the http://localhost:{port}");

while (true)
{
    var client = await server.GetContextAsync();

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
