using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        // Very small in-memory user store for demo purposes only
        private static readonly Dictionary<string, (string PasswordHash, string Email)> _users = new();

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            if (_users.ContainsKey(username))
            {
                ModelState.AddModelError("", "User already exists.");
                return View();
            }

            // Really simple hash for example only (do NOT use in production)
            var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            _users[username] = (passwordHash, email);

            return RedirectToAction("Login");
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            if (!_users.TryGetValue(username, out var info))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var attempted = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            if (attempted != info.PasswordHash)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, info.Email ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
