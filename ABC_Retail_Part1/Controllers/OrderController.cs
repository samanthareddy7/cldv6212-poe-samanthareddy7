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

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _tableService.GetAllAsync<Order>(TableName);
            return View(orders);
        }

        // GET: Create
        public IActionResult Create()
        {
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
            order.OrderDate = DateTime.Now; order.OrderDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);


            await _tableService.InsertOrUpdateAsync(TableName, order);

            // Optional: log or notify
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

            // Update fields
            existing.CustomerName = model.CustomerName;
            existing.ProductName = model.ProductName;
            existing.Quantity = model.Quantity;

            if (model.OrderDate != null)
            {
                existing.OrderDate = DateTime.SpecifyKind(model.OrderDate.Value, DateTimeKind.Utc);
            }



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
    }
}
