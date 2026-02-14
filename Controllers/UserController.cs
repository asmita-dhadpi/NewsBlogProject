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
    [AuthorizeRole("Admin", "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly NewsBlogDbContext _context;

        public UserController(NewsBlogDbContext context)
        {
            _context = context;
        }

        // ================== LIST ==================
        public IActionResult Index()
        {
            string role = HttpContext.Session.GetString("Role");

            var users = _context.TblUsers
                .Include(u => u.Role)
                .Where(u => u.IsDeleted == false);

            if (role == "Admin")
            {
                // Admin cannot see Admin/SuperAdmin
                users = users.Where(u => u.Role.RoleName == "User");
            }

            return View(users.ToList());
        }

        // ================== CREATE ==================
        public IActionResult Create()
        {
            GetRolelist();
            return View();
        }
        private void GetRolelist()
        {
            string role = HttpContext.Session.GetString("Role");

            var roles = _context.TblRoles
                .Where(r => r.IsActive);

            if (role == "Admin")
            {
                roles = roles.Where(r => r.RoleName == "User");
            }
            else
            {
                roles = roles.Where(r => r.RoleName != "SuperAdmin");
            }

            ViewBag.Roles = roles.ToList();
        }
        [HttpPost]
        public IActionResult Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                    });

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}");
                    foreach (var msg in error.Errors)
                    {
                        Console.WriteLine($" - {msg}");
                    }
                }
                GetRolelist();
                return View(model);
            }


            // Password validation
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                GetRolelist();
                return View(model);
            }

            // Email duplicate check
            bool emailExists = _context.TblUsers
                .Any(u => u.Email == model.Email && u.IsDeleted == false);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                GetRolelist();
                return View(model);
            }

            int? currentUserId = HttpContext.Session.GetInt32("UserId");

            // Create hash & salt
            PasswordHelper.CreatePasswordHash(
                model.Password,
                out string hash,
                out string salt
            );

            var user = new TblUser
            {
                RoleId = model.RoleId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now,
                CreatedBy = currentUserId,
            };

            _context.TblUsers.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================== EDIT ==================
        public IActionResult Edit(int id)
        {
            string role = HttpContext.Session.GetString("Role");

            var user = _context.TblUsers
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == id && u.IsDeleted == false);

            if (user == null)
                return RedirectToAction("UnauthorizedAccess", "Account");

            if (role == "Admin" && user.Role.RoleName != "User")
                return RedirectToAction("UnauthorizedAccess", "Account");

            var model = new UserViewModel
            {
                UserId = user.UserId,
                FirstName= user.FirstName,
                LastName=user.LastName,
                Email = user.Email,
                Password=user.PasswordHash,
                PhoneNumber=user.PhoneNumber,
                RoleId=user.RoleId,
                
            };
            GetRolelist();
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                    });

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}");
                    foreach (var msg in error.Errors)
                    {
                        Console.WriteLine($" - {msg}");
                    }
                }
                GetRolelist();
                return View(model);
            }

            var user = _context.TblUsers
                .FirstOrDefault(u => u.UserId == model.UserId && u.IsDeleted == false);

            if (user == null)
                return RedirectToAction("UnauthorizedAccess", "Account");

            bool emailExists = _context.TblUsers
               .Any(u => u.Email == model.Email && u.UserId != model.UserId && u.IsDeleted == false);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already registered");
                GetRolelist();
                return View(model);
            }
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                PasswordHelper.CreatePasswordHash(
                    model.Password,
                    out string hash,
                    out string salt
                );

                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            user.RoleId = model.RoleId;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = currentUserId;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        // ================== DELETE (SOFT) ==================
        public IActionResult Delete(int id)
        {
            var user = _context.TblUsers.FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return RedirectToAction("UnauthorizedAccess", "Account");

            user.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================== ACTIVATE / INACTIVATE ==================
        public IActionResult ToggleActive(int id)
        {
            var user = _context.TblUsers.FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return RedirectToAction("UnauthorizedAccess", "Account");

            user.IsActive = !user.IsActive;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
