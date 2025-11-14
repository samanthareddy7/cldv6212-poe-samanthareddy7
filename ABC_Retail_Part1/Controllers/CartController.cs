using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ABC_Retail_Part1.Services;
using ABC_Retail_Part1.Models;

namespace ABC_Retail_Part1.Controllers
{
    public class CartController : Controller
    {
        private readonly TableService _tableService;
        private const string CartSessionKey = "CartItems";

        public CartController(IConfiguration config, TableService tableService)
        {
            _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        }

        // --- Typed session user ---
        private UserSession? GetCurrentUser()
        {
            var s = HttpContext.Session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(s)) return null;
            return JsonSerializer.Deserialize<UserSession>(s);
        }

        public class UserSession
        {
            public int CustomerId { get; set; }
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
        }

        // --- Cart helpers ---
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json)!;
        }

        private void SaveCart(List<CartItem> items)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
        }

        public class CartItem
        {
            public string ProductId { get; set; } = "";
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        // --- Add to cart ---
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            var user = GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Product") });

            if (string.IsNullOrEmpty(productId))
            {
                TempData["Error"] = "Invalid product.";
                return RedirectToAction("Index", "Product");
            }

            const string tableName = "Products";
            const string partition = "PRODUCT";

            Product? product = await _tableService.GetAsync<Product>(tableName, partition, productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Product");
            }

            var cart = GetCart();
            var existing = cart.Find(c => c.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = product.ProductName ?? "Unnamed Product",
                    Quantity = quantity,
                    UnitPrice = Convert.ToDecimal(product.Price)
                });
            }

            SaveCart(cart);
            return RedirectToAction("ViewCart");
        }

        // --- View cart ---
        [HttpGet]
        public IActionResult ViewCart()
        {
            var user = GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ViewCart", "Cart") });

            var cart = GetCart();
            return View(cart);
        }

        // --- Checkout: creates Table Storage orders ---
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var user = GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = GetCart();
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("ViewCart");
            }

            try
            {
                foreach (var item in cart)
                {
                    var order = new Order
                    {
                        PartitionKey = "ORDER",
                        RowKey = Guid.NewGuid().ToString(),
                        CustomerName = user.Name,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    await _tableService.InsertOrUpdateAsync("Orders", order);
                }

                HttpContext.Session.Remove(CartSessionKey);
                TempData["Message"] = "Order created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Checkout failed: " + ex.Message;
            }

            return RedirectToAction("ViewCart");
        }
    }
}
