using Azure;
using Azure.Data.Tables;

namespace ABC_Retail_Part1.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Business fields
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string CustomerName => Name;





    }
}