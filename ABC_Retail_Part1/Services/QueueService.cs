using Azure.Storage.Queues;


namespace ABC_Retail_Part1.Services
{

    public class QueueService
    {
        private readonly QueueServiceClient _client;

        public QueueService(string connectionString)
        {
            _client = new QueueServiceClient(connectionString);
        }

        public QueueClient GetQueue(string queueName)
        {
            var queue = _client.GetQueueClient(queueName);
            queue.CreateIfNotExists();
            return queue;
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            var queue = GetQueue(queueName);
            await queue.SendMessageAsync(message);
        }

        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
        {
            var queue = GetQueue(queueName);
            await queue.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}