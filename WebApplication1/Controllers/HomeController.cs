using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebApplication1.Data.IProductRepository _repo;
        private readonly IWebHostEnvironment _env;

        public HomeController(WebApplication1.Data.IProductRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public IActionResult Index()
        {
            var products = Enumerable.Empty<WebApplication1.Models.Product>();
            if (User?.Identity?.IsAuthenticated == true)
            {
                products = _repo.GetAll();
            }

            ViewData["Products"] = products;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(string name, string? description, decimal price, List<IFormFile>? images)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Name is required.");
                return View();
            }

            var product = new WebApplication1.Models.Product
            {
                Name = name,
                Description = description,
                Price = price
            };

            if (images != null && images.Count > 0)
            {
                product.ImagePaths = SaveUploadedImages(images);
            }

            _repo.Add(product);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var product = _repo.Get(id);
            if (product == null)
                return NotFound();

            ViewData["Product"] = product;
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(int id, string name, string? description, decimal price, List<IFormFile>? images, string[]? removeImagePaths)
        {
            var existing = _repo.Get(id);
            if (existing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Name is required.");
                ViewData["Product"] = existing;
                return View();
            }

            existing.Name = name;
            existing.Description = description;
            existing.Price = price;

            // remove any images requested
            if (removeImagePaths != null && removeImagePaths.Length > 0)
            {
                foreach (var rp in removeImagePaths)
                {
                    if (string.IsNullOrWhiteSpace(rp)) continue;
                    existing.ImagePaths.Remove(rp);
                    try
                    {
                        var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, rp.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // ignore errors for demo
                    }
                }
            }

            // add newly uploaded images
            if (images != null && images.Count > 0)
            {
                var added = SaveUploadedImages(images);
                if (added?.Count > 0)
                    existing.ImagePaths.AddRange(added);
            }

            _repo.Update(existing);
            return RedirectToAction("Index");
        }

        private List<string> SaveUploadedImages(IEnumerable<IFormFile> images)
        {
            var saved = new List<string>();
            try
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? string.Empty, "uploads", "products");
                if (!Directory.Exists(uploadsRoot))
                    Directory.CreateDirectory(uploadsRoot);

                foreach (var file in images)
                {
                    if (file == null || file.Length == 0) continue;
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var dest = Path.Combine(uploadsRoot, fileName);
                    using (var fs = System.IO.File.Create(dest))
                    {
                        file.CopyTo(fs);
                    }
                    // store web-relative path
                    saved.Add($"/uploads/products/{fileName}");
                }
            }
            catch
            {
                // ignore errors for demo
            }
            return saved;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
