using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using ABC_Retail_Part1.Models;
using ABC_Retail_Part1.Services;

namespace ABC_Retail_Part1.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableService _tableService;
        private readonly BlobService _blobService;
        private const string TableName = "Products";
        private const string ContainerName = "productimages";

        public ProductController(TableService tableService, BlobService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllAsync<Product>(TableName);
            return View(products);
        }

        // GET: Create
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                product.PartitionKey = "PRODUCT";
                product.RowKey = Guid.NewGuid().ToString();

                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await _blobService.UploadFileAsync(imageFile);
                }

                await _tableService.InsertAsync(TableName, product);
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();

            var product = await _tableService.GetAsync<Product>(TableName, "PRODUCT", rowKey);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, Product model, IFormFile imageFile)
        {
            if (rowKey != model.RowKey) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _tableService.GetAsync<Product>(TableName, "PRODUCT", rowKey);
            if (existing == null) return NotFound();

            // Update fields
            existing.ProductName = model.ProductName;
            existing.Description = model.Description;
            existing.Price = model.Price;

            if (imageFile != null && imageFile.Length > 0)
            {
                var blobName = $"{existing.RowKey}{Path.GetExtension(imageFile.FileName)}";
                using var stream = imageFile.OpenReadStream();

                await _blobService.UploadBlobAsync(ContainerName, blobName, stream);
                existing.ImageUrl = _blobService.GetBlobUrl(ContainerName, blobName);
            }

            await _tableService.UpdateAsync(TableName, existing);
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
            var product = await _tableService.GetAsync<Product>(TableName, "PRODUCT", rowKey);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            var product = await _tableService.GetAsync<Product>(TableName, "PRODUCT", rowKey);

            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                var blobName = Path.GetFileName(new Uri(product.ImageUrl).LocalPath);
                await _blobService.DeleteBlobAsync(ContainerName, blobName);
            }

            await _tableService.DeleteAsync(TableName, "PRODUCT", rowKey);
            return RedirectToAction(nameof(Index));
        }

        // GET: Details
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey)) return NotFound();
            var product = await _tableService.GetAsync<Product>(TableName, "PRODUCT", rowKey);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
