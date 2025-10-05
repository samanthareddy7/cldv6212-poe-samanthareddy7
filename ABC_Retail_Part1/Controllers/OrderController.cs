using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Models;
using ABC_Retail_Part1.Services;
using System;
using System.Threading.Tasks;

namespace ABC_Retail_Part1.Controllers
{
    public class OrderController : Controller
    {
        private readonly TableService _tableService;
        private readonly QueueService _queueService;
        private const string TableName = "Orders";
        private const string Partition = "ORDER";

        public OrderController(TableService tableService, QueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _tableService.GetAllAsync<Order>(TableName);

            // pull some messages to show on the page
            var messages = await _queueService.GetMessagesAsync("orderqueue", maxMessages: 20, peekOnly: true);

            var vm = new OrderIndexViewModel
            {
                Orders = orders,
                QueueMessages = messages
            };

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            var customers = await _tableService.GetAllAsync<Customer>("Customer");

            ViewBag.CustomerList = customers
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CustomerName,   // or c.RowKey if you want IDs
                    Text = c.CustomerName     // show the name in dropdown
                })
                .ToList();

            return View();
        }


        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            order.PartitionKey = Partition;
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            // Insert into Azure Table
            await _tableService.InsertOrUpdateAsync(TableName, order);

            // Send a message to the queue with correct local time
            if (_queueService != null)
            {
                string orderTime = order.OrderDate.HasValue
                    ? order.OrderDate.Value.ToLocalTime().ToString("g")
                    : "N/A";

                string message = $"New order placed: Customer={order.CustomerName}, Product={order.ProductName}, Quantity={order.Quantity}, Date={orderTime}";
                await _queueService.SendMessageAsync("orderqueue", message);
            }


            Console.WriteLine($"Order created: {order.ProductName}, Quantity: {order.Quantity}");

            return RedirectToAction(nameof(Index));
        }

        // GET: Details
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();

            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();

            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, Order model)
        {
            if (rowKey != model.RowKey) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (existing == null) return NotFound();

            existing.CustomerName = model.CustomerName;
            existing.ProductName = model.ProductName;
            existing.Quantity = model.Quantity;

            if (model.OrderDate != null)
                existing.OrderDate = DateTime.SpecifyKind(model.OrderDate.Value, DateTimeKind.Utc);

            await _tableService.UpdateAsync(TableName, existing);
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();

            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            await _tableService.DeleteAsync(TableName, Partition, rowKey);
            return RedirectToAction(nameof(Index));
        }

        // GET: QueueMessages
        public async Task<IActionResult> QueueMessages()
        {
            if (_queueService == null)
                return BadRequest("Queue service not available.");

            var messages = await _queueService.GetMessagesAsync("orderqueue");
            return View(messages);
        }
    }
}
