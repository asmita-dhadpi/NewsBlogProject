using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlogProject.Data;
using NewsBlogProject.Filters;
using NewsBlogProject.Models;
using NewsBlogProject.ViewModels;
using System.Security.Claims;

namespace NewsBlogProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly NewsBlogDbContext _context;

        public AccountController(NewsBlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.TblUsers
                .Include(u => u.Role)
                .FirstOrDefault(u =>
                    u.Email == model.Email &&
                    u.IsActive == true &&
                    u.IsDeleted == false);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            bool isPasswordValid = PasswordHelper.VerifyPassword(
                model.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName) // SuperAdmin / Admin / User
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

           
            // Store session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool emailExists = _context.TblUsers
                .Any(u => u.Email == model.Email && u.IsDeleted == false);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already registered");
                return View(model);
            }

            // Create hash & salt
            PasswordHelper.CreatePasswordHash(
                model.Password,
                out string hash,
                out string salt
            );
            int userRoleId = _context.TblRoles
                       .Where(r => r.RoleName == "User")
                       .Select(r => r.RoleId)
                       .First();

            var user = new TblUser
            {
                RoleId = userRoleId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            };
            _context.TblUsers.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
            [AuthorizeRole]
        public IActionResult Profile()
        {
            return View();
        }
    }
}
