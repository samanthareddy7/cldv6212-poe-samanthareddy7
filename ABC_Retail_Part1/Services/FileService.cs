using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Net.Http.Headers;

namespace ABC_Retail_Part1.Services
{
    public class FileService
    {
        private readonly ShareServiceClient _client;
        private static readonly HttpClient _http = new HttpClient();

        public FileService(string connectionString)
        {
            _client = new ShareServiceClient(connectionString);
        }

        public ShareClient GetShare(string name)
        {
            var share = _client.GetShareClient(name);
            share.CreateIfNotExists();
            return share;
        }

        // Upload through Azure Function
        public async Task<bool> UploadFileAsync(string customerName, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(customerName)) customerName = "general";
            if (file == null || file.Length == 0) return false;

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var url = $"http://localhost:7073/api/contracts/{Uri.EscapeDataString(customerName)}";
            var response = await _http.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<string>> ListFilesAsync(string shareName, string customerName)
        {
            customerName = string.IsNullOrWhiteSpace(customerName) ? "general" : customerName;
            var share = GetShare(shareName);
            var root = share.GetRootDirectoryClient();
            var dir = root.GetSubdirectoryClient(customerName);
            await dir.CreateIfNotExistsAsync();

            var list = new List<string>();
            await foreach (ShareFileItem f in dir.GetFilesAndDirectoriesAsync())
                list.Add(f.Name);
            return list;
        }

        public async Task<Stream> DownloadFileAsync(string shareName, string customerName, string fileName)
        {
            var share = GetShare(shareName);
            var dir = share.GetRootDirectoryClient().GetSubdirectoryClient(customerName ?? "general");
            var file = dir.GetFileClient(fileName);

            var download = await file.DownloadAsync();
            var memory = new MemoryStream();
            await download.Value.Content.CopyToAsync(memory);
            memory.Position = 0;
            return memory;
        }

        public async Task DeleteFileAsync(string shareName, string customerName, string fileName)
        {
            var share = GetShare(shareName);
            var dir = share.GetRootDirectoryClient().GetSubdirectoryClient(customerName ?? "general");
            var file = dir.GetFileClient(fileName);
            await file.DeleteIfExistsAsync();
        }
    }
}
