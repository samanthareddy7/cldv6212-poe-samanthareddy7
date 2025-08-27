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
        private const string TableName = "Orders";
        private const string Partition = "ORDER";

        public OrderController(TableService tableService)
        {
            _tableService = tableService;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _tableService.GetAllAsync<Order>(TableName);
            return View(orders);
        }

        // GET: /Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            order.PartitionKey = Partition;
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.UtcNow;

            await _tableService.InsertOrUpdateAsync(TableName, order);

            // Send a notification (log to console or save a message)
            Console.WriteLine($"Order created: {order.ProductName}, Quantity: {order.Quantity}");

            return RedirectToAction(nameof(Index));
        }

        // GET: /Orders/Details/{rowKey}
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Orders/Edit/{rowKey}
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Orders/Edit/{rowKey}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, Order model)
        {
            if (rowKey != model.RowKey) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            model.PartitionKey = Partition;
            await _tableService.UpdateAsync(TableName, model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Orders/Delete/{rowKey}
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
            var order = await _tableService.GetAsync<Order>(TableName, Partition, rowKey);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Orders/DeleteConfirmed/{rowKey}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            await _tableService.DeleteAsync(TableName, Partition, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
