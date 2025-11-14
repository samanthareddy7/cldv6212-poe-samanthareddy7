using Azure;
using Azure.Data.Tables;

namespace ABC_Retail_Part1.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Business fields
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }

        // New: Blob URL
        public string ImageUrl { get; set; } = string.Empty;
    }
}