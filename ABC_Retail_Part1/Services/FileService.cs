using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABC_Retail_Part1.Services
{
    public class FileService
    {
        private readonly ShareServiceClient _client;

        public FileService(string connectionString)
        {
            _client = new ShareServiceClient(connectionString);
        }

        public ShareClient GetShare(string shareName)
        {
            var share = _client.GetShareClient(shareName);
            share.CreateIfNotExists();
            return share;
        }

        // Upload a file into a customer folder
        public async Task UploadFileAsync(string shareName, string customerName, string fileName, Stream fileStream)
        {
            customerName = string.IsNullOrWhiteSpace(customerName) ? "general" : customerName;
            var share = GetShare(shareName);
            var rootDir = share.GetRootDirectoryClient();
            var customerDir = rootDir.GetSubdirectoryClient(customerName);
            await customerDir.CreateIfNotExistsAsync();

            var fileClient = customerDir.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        // List files in a customer folder
        public async Task<List<string>> ListFilesAsync(string shareName, string customerName)
        {
            customerName = string.IsNullOrWhiteSpace(customerName) ? "general" : customerName;
            var share = GetShare(shareName);
            var rootDir = share.GetRootDirectoryClient();
            var customerDir = rootDir.GetSubdirectoryClient(customerName);
            await customerDir.CreateIfNotExistsAsync();

            var files = new List<string>();
            await foreach (ShareFileItem f in customerDir.GetFilesAndDirectoriesAsync())
            {
                files.Add(f.Name);
            }

            return files;
        }

        // Download file
        public async Task<Stream> DownloadFileAsync(string shareName, string customerName, string fileName)
        {
            customerName = string.IsNullOrWhiteSpace(customerName) ? "general" : customerName;
            var share = GetShare(shareName);
            var rootDir = share.GetRootDirectoryClient();
            var customerDir = rootDir.GetSubdirectoryClient(customerName);
            var fileClient = customerDir.GetFileClient(fileName);

            var download = await fileClient.DownloadAsync();
            var memoryStream = new MemoryStream();
            await download.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        // Delete file
        public async Task DeleteFileAsync(string shareName, string customerName, string fileName)
        {
            customerName = string.IsNullOrWhiteSpace(customerName) ? "general" : customerName;
            var share = GetShare(shareName);
            var rootDir = share.GetRootDirectoryClient();
            var customerDir = rootDir.GetSubdirectoryClient(customerName);
            var fileClient = customerDir.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
}
