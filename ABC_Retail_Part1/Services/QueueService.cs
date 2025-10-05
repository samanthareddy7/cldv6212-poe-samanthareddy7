using System.Text;
using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace ABC_Retail_Part1.Services
{
    public class QueueService
    {
        private readonly QueueServiceClient _client;
        private static readonly HttpClient _http = new HttpClient();

        public QueueService(string connectionString)
        {
            _client = new QueueServiceClient(connectionString);
        }

        private QueueClient GetQueue(string name)
        {
            var queue = _client.GetQueueClient(name);
            queue.CreateIfNotExists();
            return queue;
        }

        // ✅ Send message via HTTP Function (not directly to Azure Queue)
        public async Task<bool> SendMessageAsync(string queueName, object message)
        {
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "https://st10454507.azurewebsites.net/api/orders"; // Matches your Function endpoint
            var response = await _http.PostAsync(url, content);

            return response.IsSuccessStatusCode;
        }

        // ✅ Retrieve messages directly from Azure Queue
        public async Task<List<string>> GetMessagesAsync(string queueName, int maxMessages = 20, bool peekOnly = true)
        {
            var queue = GetQueue(queueName);
            var list = new List<string>();

            if (peekOnly)
            {
                var peeked = await queue.PeekMessagesAsync(maxMessages);
                foreach (var m in peeked.Value)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(m.MessageText));
                    list.Add(decoded);
                }
            }
            else
            {
                var received = await queue.ReceiveMessagesAsync(maxMessages);
                foreach (var m in received.Value)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(m.MessageText));
                    list.Add(decoded);
                    await queue.DeleteMessageAsync(m.MessageId, m.PopReceipt);
                }
            }

            return list;
        }
    }
}
