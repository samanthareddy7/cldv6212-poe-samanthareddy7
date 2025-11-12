using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Files.Shares;

public class FileFunction
{
    [Function("UploadContract")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contracts/{customerName}")] HttpRequestData req,
        string customerName)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var share = new ShareClient(conn, "contracts");
        await share.CreateIfNotExistsAsync();

        var dir = share.GetRootDirectoryClient().GetSubdirectoryClient(customerName ?? "general");
        await dir.CreateIfNotExistsAsync();

        var fileName = $"{Guid.NewGuid()}.txt";
        var fileClient = dir.GetFileClient(fileName);

        using var mem = new MemoryStream();
        await req.Body.CopyToAsync(mem);
        mem.Position = 0;

        await fileClient.CreateAsync(mem.Length);
        await fileClient.UploadAsync(mem);

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync($"Contract uploaded for {customerName}.");
        return res;
    }
}
