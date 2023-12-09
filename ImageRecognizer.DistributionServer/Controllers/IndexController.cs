using System.Net;

namespace ImageRecognizer.DistributionServer.Controllers;

public class IndexController : ControllerBase
{
    public override string[] RawUrls {  get; init; }

    public IndexController()
    {
        RawUrls = new string[] { "/", "/styles.css", "/script.js" };
    }

    public override async Task ProcessRequestAsync(HttpListenerContext client)
    {
        var request = client.Request;

        if (request.HttpMethod == HttpMethod.Get.Method)
        {
            if (request.RawUrl == "/")
            {
                SendFile(client, "wwwroot/index.html", "text/html");
            }
            else if (request.RawUrl == "/styles.css")
            {
                SendFile(client, "wwwroot/styles.css", "text/css");
            }
            else if (request.RawUrl == "/script.js")
            {
                SendFile(client, "wwwroot/script.js", "text/javascript");
            }
        }

        client.Response.Close();
    }

    private void SendFile(HttpListenerContext context, string path, string type)
    {
        using HttpListenerResponse response = context.Response;
        response.Headers.Set("Content-Type", type);

        byte[] buf = File.ReadAllBytes(path);
        response.ContentLength64 = buf.Length;

        using Stream ros = response.OutputStream;
        ros.Write(buf, 0, buf.Length);
    }
}
