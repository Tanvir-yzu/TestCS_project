using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebApplication1.Data.IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public AccountController(WebApplication1.Data.IUserRepository userRepo, IWebHostEnvironment env)
        {
            _userRepo = userRepo;
            _env = env;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            // Preserve entered values when redisplaying the form on error
            ViewData["Username"] = username;
            ViewData["Email"] = email;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (_userRepo.GetByUsername(username) != null)
            {
                ModelState.AddModelError("", "User already exists.");
                return View();
            }

            // Ensure email is unique
            if (!string.IsNullOrWhiteSpace(email) && _userRepo.GetByEmail(email) != null)
            {
                ModelState.AddModelError("", "Email already in use.");
                return View();
            }

            // Really simple hash for example only (do NOT use in production)
            var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            var user = new WebApplication1.Models.User
            {
                Username = username,
                Email = email ?? string.Empty,
                PasswordHash = passwordHash
            };

            _userRepo.Add(user);

            return RedirectToAction("Login");
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Email"] = email;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var existing = _userRepo.GetByEmail(email);
            if (existing == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var attempted = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            if (attempted != existing.PasswordHash)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existing.Username),
                new Claim(ClaimTypes.Email, existing.Email ?? string.Empty)
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

        [HttpGet]
        public IActionResult Profile()
        {
            if (!User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Login");

            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return RedirectToAction("Login");

            var user = _userRepo.GetByUsername(username!);
            if (user == null) return NotFound();

            ViewData["User"] = user;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string? newUsername, IFormFile? avatar)
        {
            if (!User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Login");

            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return RedirectToAction("Login");

            var user = _userRepo.GetByUsername(username!);
            if (user == null) return NotFound();

            var oldUsername = user.Username;

            // handle username change
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != oldUsername)
            {
                // ensure new username not taken
                if (_userRepo.GetByUsername(newUsername) != null)
                {
                    ModelState.AddModelError("", "Username already taken.");
                    ViewData["User"] = user;
                    return View();
                }

                user.Username = newUsername;
            }

            if (avatar != null && avatar.Length > 0)
            {
                try
                {
                    var uploads = Path.Combine(_env.WebRootPath ?? string.Empty, "uploads", "avatars");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var ext = Path.GetExtension(avatar.FileName);
                    var fn = Guid.NewGuid().ToString("N") + ext;
                    var dest = Path.Combine(uploads, fn);
                    using (var fs = System.IO.File.Create(dest))
                    {
                        avatar.CopyTo(fs);
                    }
                    user.AvatarPath = $"/uploads/avatars/{fn}";
                    _userRepo.Update(user, oldUsername);
                }
                catch
                {
                    // ignore for demo
                }
            }
            else
            {
                // if only username changed
                if (user.Username != oldUsername)
                {
                    _userRepo.Update(user, oldUsername);
                }
            }

            // if username changed we need to refresh auth cookie
            if (user.Username != oldUsername)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            ViewData["User"] = user;
            return View();
        }
    }
}
