using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;

public class BlobFunction
{
    [Function("UploadProductImage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadimage")] HttpRequestData req)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var blobService = new BlobServiceClient(conn);
        var container = blobService.GetBlobContainerClient("productimages");
        await container.CreateIfNotExistsAsync();

        var blobName = Guid.NewGuid().ToString() + ".jpg";
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(req.Body, overwrite: true);

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync($"https://{blobService.AccountName}.blob.core.windows.net/productimages/{blobName}");
        return res;
    }
}
