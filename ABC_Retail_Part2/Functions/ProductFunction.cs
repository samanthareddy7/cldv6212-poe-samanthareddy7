using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Data.Tables;
using Newtonsoft.Json;

public class ProductFunction
{
    [Function("AddProduct")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "Products");
        await table.CreateIfNotExistsAsync();

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

        var entity = new TableEntity("Products", Guid.NewGuid().ToString());
        foreach (var kv in data)
            entity[kv.Key] = kv.Value?.ToString();

        await table.AddEntityAsync(entity);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"? Product added: {entity["ProductName"]}");
        return response;
    }
}
