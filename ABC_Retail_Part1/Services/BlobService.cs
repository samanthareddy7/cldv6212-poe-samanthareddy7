using Azure.Storage.Blobs;

namespace ABC_Retail_Part1.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _client;
        private readonly string _accountName;

        public BlobService(string connectionString)
        {
            _client = new BlobServiceClient(connectionString);

            // extract account name from connection string
            _accountName = connectionString
                .Split(';')
                .FirstOrDefault(p => p.StartsWith("AccountName="))?
                .Split('=')[1] ?? "";
        }

        public BlobContainerClient GetContainer(string containerName)
        {
            var container = _client.GetBlobContainerClient(containerName);
            container.CreateIfNotExists();
            return container;
        }

        public async Task UploadBlobAsync(string containerName, string fileName, Stream fileStream)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(fileName);
            await blob.UploadAsync(fileStream, true);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName = "productimages")
        {
            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            using var stream = file.OpenReadStream();
            await UploadBlobAsync(containerName, blobName, stream);

            return GetBlobUrl(containerName, blobName);
        }

        public async Task DeleteBlobAsync(string containerName, string fileName)
        {
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<List<string>> ListBlobsAsync(string containerName)
        {
            var container = GetContainer(containerName);
            var blobs = new List<string>();
            await foreach (var blob in container.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }
            return blobs;
        }

        // ✅ Add this method to fix your controller
        public string GetBlobUrl(string containerName, string blobName)
        {
            return $"https://{_accountName}.blob.core.windows.net/{containerName}/{blobName}";
        }
    }
}
