using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Services;
using ABC_Retail_Part1.Models;

namespace ABC_Retail_Part1.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IActionResult> Upload(string customerName = "general")
        {
            var files = await _fileService.ListFilesAsync("contracts", customerName);

            // Create a ViewModel with file names + customer
            var fileList = files.Select(f => new FileViewModel
            {
                FileName = f,
                CustomerName = customerName
            }).ToList();

            ViewBag.CustomerName = customerName;
            return View(fileList);
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string customerName)
        {
            if (file != null && file.Length > 0)
            {
                await _fileService.UploadFileAsync(customerName, file); // ✅ only 2 arguments now
            }
            return RedirectToAction("Upload", new { customerName });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(string customerName, string fileName)
        {
            await _fileService.DeleteFileAsync("contracts", customerName, fileName);
            return RedirectToAction("Upload", new { customerName });
        }

        public async Task<FileResult> DownloadFile(string customerName, string fileName)
        {
            var stream = await _fileService.DownloadFileAsync("contracts", customerName, fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
