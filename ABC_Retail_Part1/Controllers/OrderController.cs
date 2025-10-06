using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Models;
using ABC_Retail_Part1.Services;
using System;
using System.Linq;
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

        // GET: Orders (with search)
        public async Task<IActionResult> Index(string searchTerm)
        {
            var orders = await _tableService.GetAllAsync<Order>(TableName);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                orders = orders
                    .Where(o =>
                        (!string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ProductName) && o.ProductName.ToLower().Contains(searchTerm)))
                    .ToList();
            }

            // queue messages (optional)
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
                    Value = c.CustomerName,
                    Text = c.CustomerName
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            order.PartitionKey = Partition;
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";

            await _tableService.InsertOrUpdateAsync(TableName, order);

            // queue
            if (_queueService != null)
            {
                string message = $"New order placed: {order.CustomerName} - {order.ProductName}";
                await _queueService.SendMessageAsync("orderqueue", message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string rowKey, string newStatus)
        {
            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();

            order.Status = newStatus;
            await _tableService.UpdateAsync(TableName, order);

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