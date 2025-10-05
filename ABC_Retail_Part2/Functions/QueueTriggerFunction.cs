using Microsoft.Azure.Functions.Worker;
using Azure.Data.Tables;
using Newtonsoft.Json;

public class QueueTriggerFunction
{
    [Function("ProcessOrder")]
    public async Task Run([QueueTrigger("orderqueue", Connection = "AzureWebJobsStorage")] string message)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "Orders");
        await table.CreateIfNotExistsAsync();

        var entity = new TableEntity("Orders", Guid.NewGuid().ToString())
    {
        { "Message", message }
    };

        await table.AddEntityAsync(entity);
    }
}