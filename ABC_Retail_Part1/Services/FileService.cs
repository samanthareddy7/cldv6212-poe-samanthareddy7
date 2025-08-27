using Azure.Storage.Files.Shares;

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

        public async Task UploadFileAsync(string shareName, string fileName, Stream fileStream)
        {
            var share = GetShare(shareName);
            var root = share.GetRootDirectoryClient();
            var file = root.GetFileClient(fileName);
            await file.CreateAsync(fileStream.Length);
            await file.UploadAsync(fileStream);
        }

        public async Task<List<string>> ListFilesAsync(string shareName)
        {
            var share = GetShare(shareName);
            var root = share.GetRootDirectoryClient();
            var files = new List<string>();
            await foreach (var f in root.GetFilesAndDirectoriesAsync())
            {
                files.Add(f.Name);
            }
            return files;
        }
    }
}