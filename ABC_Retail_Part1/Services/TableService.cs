using Azure;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABC_Retail_Part1.Services
{
    public class TableService
    {
        private readonly string _connectionString;

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

        // Insert a new entity
        public async Task InsertAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            await table.AddEntityAsync(entity);
        }

        // Insert or update an entity (upsert)
        public async Task InsertOrUpdateAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            await table.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        // Get all entities
        public async Task<List<T>> GetAllAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            var entities = table.QueryAsync<T>();
            var results = new List<T>();

            await foreach (var e in entities)
            {
                results.Add(e);
            }

            return results;
        }

        // Get a single entity by PartitionKey and RowKey
        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
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

        // Update an existing entity
        public async Task UpdateAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var table = GetTable(tableName);
            await table.UpdateEntityAsync(entity, ETag.All);
        }

        // Delete an entity
        public async Task DeleteAsync(string tableName, string partitionKey, string rowKey)
        {
            var table = GetTable(tableName);
            await table.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
