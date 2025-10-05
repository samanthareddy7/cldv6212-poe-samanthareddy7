using System.Net.Http.Headers;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABC_Retail_Part1.Services
{
    public class FileService
    {
        private readonly ShareServiceClient _client;
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public FileService(string connectionString, string baseUrl)
        {
            _client = new ShareServiceClient(connectionString);
            _http = new HttpClient();
            _baseUrl = baseUrl.TrimEnd('/'); 
        }

        public ShareClient GetShare(string name)
        {
            var share = _client.GetShareClient(name);
            share.CreateIfNotExists();
            return share;
        }

        //  Upload through Azure Function
        public async Task<bool> UploadFileAsync(string customerName, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(customerName)) customerName = "general";
            if (file == null || file.Length == 0) return false;

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Headers.Add("x-filename", file.FileName);

            var url = $"https://st10454507.azurewebsites.net/api/contracts/{Uri.EscapeDataString(customerName)}";
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
