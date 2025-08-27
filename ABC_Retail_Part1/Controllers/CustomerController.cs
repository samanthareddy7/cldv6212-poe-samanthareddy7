using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Models;
using ABC_Retail_Part1.Services;
using System;
using System.Threading.Tasks;


namespace ABC_Retail_Part1.Controllers
    {
        public class CustomerController : Controller
        {
            private readonly TableService _tableService;
            private const string TableName = "Customer";
            private const string Partition = "Customer";

            public CustomerController(TableService tableService)
            {
                _tableService = tableService;
            }

            // GET: /Customer
            public async Task<IActionResult> Index()
            {
                var customers = await _tableService.GetAllAsync<Customer>(TableName);
                return View(customers);
            }

            // GET: /Customer/Create
            public IActionResult Create() => View();

            // POST: /Customer/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Customer model)
            {
                if (!ModelState.IsValid) return View(model);

                model.PartitionKey = Partition;
                model.RowKey = Guid.NewGuid().ToString();
                await _tableService.InsertOrUpdateAsync(TableName, model);
                return RedirectToAction(nameof(Index));
            }

            // GET: /Customer/Edit/{rowKey}
            public async Task<IActionResult> Edit(string rowKey)
            {
                if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
                var entity = await _tableService.GetAsync<Customer>(TableName, Partition, rowKey);
                if (entity == null) return NotFound();
                return View(entity);
            }

            // POST: /Customer/Edit/{rowKey}
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(string rowKey, Customer model)
            {
                if (rowKey != model.RowKey) return BadRequest();
                if (!ModelState.IsValid) return View(model);

                model.PartitionKey = Partition;
                await _tableService.UpdateAsync(TableName, model);
                return RedirectToAction(nameof(Index));
            }

            // GET: /Customer/Delete/{rowKey}
            public async Task<IActionResult> Delete(string rowKey)
            {
                if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
                var entity = await _tableService.GetAsync<Customer>(TableName, Partition, rowKey);
                if (entity == null) return NotFound();
                return View(entity);
            }

            // POST: /Customer/DeleteConfirmed/{rowKey}
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(string rowKey)
            {
                await _tableService.DeleteAsync(TableName, Partition, rowKey);
                return RedirectToAction(nameof(Index));
            }

            // GET: /Customer/Details/{rowKey}
            public async Task<IActionResult> Details(string rowKey)
            {
                if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
                var entity = await _tableService.GetAsync<Customer>(TableName, Partition, rowKey);
                if (entity == null) return NotFound();
                return View(entity);
            }
        }
    }
