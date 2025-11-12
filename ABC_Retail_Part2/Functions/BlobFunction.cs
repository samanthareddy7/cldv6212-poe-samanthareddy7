using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;

public class BlobFunction
{
    [Function("UploadProductImage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "uploadimage")] HttpRequestData req)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var blobService = new BlobServiceClient(conn);
        var container = blobService.GetBlobContainerClient("productimages");
        await container.CreateIfNotExistsAsync();

        string fileName = "uploaded_" + Guid.NewGuid() + ".jpg"; 
        if (req.Headers.TryGetValues("Content-Disposition", out var values))
        {
            var header = values.FirstOrDefault();
            var match = Regex.Match(header ?? "", "filename=\"?([^\";]+)\"?");
            if (match.Success)
            {
                fileName = match.Groups[1].Value;
            }
        }

        var blob = container.GetBlobClient(fileName);
        await blob.UploadAsync(req.Body, overwrite: true);

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync($"https://{blobService.AccountName}.blob.core.windows.net/productimages/{fileName}");
        return res;
    }
}
