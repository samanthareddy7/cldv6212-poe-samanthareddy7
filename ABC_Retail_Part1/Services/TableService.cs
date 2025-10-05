using System.Text;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace ABC_Retail_Part1.Services
{
    public class TableService
    {
        private readonly string _connectionString;
        private static readonly HttpClient _http = new HttpClient();

        public TableService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private TableClient GetTable(string tableName)
        {
            var client = new TableClient(_connectionString, tableName);
            client.CreateIfNotExists();
            return client;
        }

        // Insert via your Function (Customers or Orders)
        public async Task<bool> InsertAsync<T>(string tableName, T entity)
        {
            var json = JsonConvert.SerializeObject(entity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string? url = tableName.ToLower() switch
            {
                "customers" => "http://localhost:7073/api/customers",
                "orders" => "http://localhost:7073/api/orders",
                "products" => "http://localhost:7073/api/products", 
                _ => null
            };


            if (url == null) throw new InvalidOperationException("No matching Function endpoint for this table.");

            var response = await _http.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        // Upsert directly to Azure Table (used by Product & Order editing)
        public async Task InsertOrUpdateAsync<T>(string tableName, T entity)
            where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            await table.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        // Get all
        public async Task<List<T>> GetAllAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            var results = new List<T>();
            await foreach (var e in table.QueryAsync<T>())
                results.Add(e);
            return results;
        }

        // Get single
        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey)
            where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            try
            {
                var response = await table.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        // Update
        public async Task UpdateAsync<T>(string tableName, T entity)
            where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            await table.UpdateEntityAsync(entity, ETag.All);
        }

        // Delete
        public async Task DeleteAsync(string tableName, string partitionKey, string rowKey)
        {
            var table = GetTable(tableName);
            await table.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
