using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Queues;

public class QueueHttpFunction
{
    [Function("PlaceOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var queue = new QueueClient(conn, "orderqueue");
        await queue.CreateIfNotExistsAsync();
        await queue.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(body)));

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync("Order added to queue.");
        return res;
    }
}
