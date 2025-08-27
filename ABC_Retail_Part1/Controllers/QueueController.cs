using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Services;
namespace ABC_Retail_Part1.Controllers
{
    

    public class QueueController : Controller
    {
        private readonly QueueService _queueService;

        public QueueController(QueueService queueService)
        {
            _queueService = queueService;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> SendMessage(string customer, string product, int quantity)
        {
            if (string.IsNullOrEmpty(customer)) customer = "DefaultCustomer";
            if (string.IsNullOrEmpty(product)) product = "DefaultProduct";

            string message = $"Customer: {customer}, Product: {product}, Quantity: {quantity}";
            await _queueService.SendMessageAsync("orderqueue", message);
            ViewBag.Message = "Message sent!";
            return View("Index");
        }
    }

}
