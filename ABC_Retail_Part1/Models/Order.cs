using Azure;
using Azure.Data.Tables;

namespace ABC_Retail_Part1.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "ORDER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public DateTime? OrderDate { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
