using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebApplication1.Data.IProductRepository _repo;

        public HomeController(WebApplication1.Data.IProductRepository repo)
        {
            _repo = repo;
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
        public IActionResult Create(string name, string? description, decimal price)
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

            _repo.Add(product);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
