using Microsoft.AspNetCore.Mvc;
using ABC_Retail_Part1.Services;

namespace ABC_Retail_Part1.Controllers
{

    public class FilesController : Controller
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IActionResult> Upload()
        {
            var files = await _fileService.ListFilesAsync("contracts");
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _fileService.UploadFileAsync("contracts", file.FileName, file.OpenReadStream());
            }
            return RedirectToAction("Upload");
        }
    }

}
