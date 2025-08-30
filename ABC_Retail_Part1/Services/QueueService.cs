using Azure.Storage.Queues;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // Send a message
        public async Task SendMessageAsync(string queueName, string message)
        {
            var queue = GetQueue(queueName);
            await queue.SendMessageAsync(message);
        }

        // Delete a message
        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
        {
            var queue = GetQueue(queueName);
            await queue.DeleteMessageAsync(messageId, popReceipt);
        }

        // Retrieve messages (peek or consume)
        public async Task<List<string>> GetMessagesAsync(string queueName, int maxMessages = 20, bool peekOnly = true)
        {
            var queue = GetQueue(queueName);
            var list = new List<string>();

            if (peekOnly)
            {
                // Show messages without consuming them
                var peeked = await queue.PeekMessagesAsync(maxMessages);
                foreach (var m in peeked.Value)
                    list.Add(m.MessageText);
            }
            else
            {
                // Consume (receive + delete) to avoid duplicates
                var received = await queue.ReceiveMessagesAsync(maxMessages);
                foreach (var m in received.Value)
                {
                    list.Add(m.MessageText);
                    await queue.DeleteMessageAsync(m.MessageId, m.PopReceipt);
                }
            }

            return list;
        }
    }
}
