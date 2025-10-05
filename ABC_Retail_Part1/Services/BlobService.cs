using Azure.Storage.Blobs;
using System.Net.Http.Headers;

namespace ABC_Retail_Part1.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _client;
        private readonly string _accountName;
        private static readonly HttpClient _http = new HttpClient();

        public BlobService(string connectionString)
        {
            _client = new BlobServiceClient(connectionString);
            _accountName = connectionString
                .Split(';')
                .FirstOrDefault(p => p.StartsWith("AccountName="))?
                .Split('=')[1] ?? "";
        }

        public BlobContainerClient GetContainer(string name)
        {
            var container = _client.GetBlobContainerClient(name);
            container.CreateIfNotExists();
            return container;
        }

        public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(blobName);
            await blob.UploadAsync(stream, true);
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }

        // Function call (for UploadProductImage)
        public async Task<string?> UploadFileAsync(IFormFile file, string containerName = "productimages")
        {
            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            var response = await _http.PostAsync("https://st10454507.azurewebsites.net/api/uploadimage", content);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            return result.Contains("http") ? result.Split(' ').Last().Trim() : result;
        }

        public string GetBlobUrl(string container, string blob)
            => $"https://{_accountName}.blob.core.windows.net/{container}/{blob}";
    }
}
