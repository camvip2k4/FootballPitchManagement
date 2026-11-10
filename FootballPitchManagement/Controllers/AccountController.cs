using System.Security.Claims;
using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == model.Username.Trim());
            if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (account.Status != AccountStatus.ACTIVE)
            {
                ModelState.AddModelError(string.Empty, "Your account is deactivated. Please contact administrator.");
                return View(model);
            }

            await SignInUserAsync(account);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Quick demo login helper for convenience
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoLogin(string role)
        {
            var targetRole = role.ToUpper() switch
            {
                "ADMIN" => RoleType.ADMIN,
                "MANAGER" => RoleType.MANAGER,
                "STAFF" => RoleType.STAFF,
                _ => RoleType.CUSTOMER
            };

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Role == targetRole);
            if (account != null)
            {
                await SignInUserAsync(account);
                TempData["SuccessMessage"] = $"Logged in as demo user: {account.FullName} ({account.Role})";

                if (account.Role == RoleType.STAFF)
                {
                    return RedirectToAction("Index", "Staff");
                }
                if (account.Role == RoleType.ADMIN || account.Role == RoleType.MANAGER)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.Accounts.AnyAsync(a => a.Username == model.Username.Trim()))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            if (await _context.Accounts.AnyAsync(a => a.Email == model.Email.Trim()))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            var account = new Account
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FullName = model.FullName.Trim(),
                Phone = model.Phone.Trim(),
                Role = RoleType.CUSTOMER,
                Status = AccountStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            await SignInUserAsync(account);
            TempData["SuccessMessage"] = "Registration successful! Welcome to Football Field Management System.";

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUserAsync(Account account)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.GivenName, account.FullName),
                new Claim(ClaimTypes.Role, account.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}

