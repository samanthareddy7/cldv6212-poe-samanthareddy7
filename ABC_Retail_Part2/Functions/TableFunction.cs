using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Data.Tables;
using Newtonsoft.Json;

public class TableFunction
{
    [Function("AddCustomer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

        var entity = new TableEntity("CUSTOMER", Guid.NewGuid().ToString());
        foreach (var kv in data)
            entity[kv.Key] = kv.Value?.ToString();

        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "Customers");
        await table.CreateIfNotExistsAsync();
        await table.AddEntityAsync(entity);

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync("Customer added successfully.");
        return res;
    }
}
