using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsBlogProject.Data;
using NewsBlogProject.Filters;
using NewsBlogProject.Models;
using NewsBlogProject.ViewModels;
using System.Data;

namespace NewsBlogProject.Controllers
{
    [AuthorizeRole("User", "Admin", "SuperAdmin")]
    public class NewsBlogController : Controller
    {
        private readonly NewsBlogDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public NewsBlogController(NewsBlogDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index(string status)
        {
            var query = _context.TblNewsBlogs
                .Include(n => n.NewsBlogStatus)
                .Include(n => n.Category)
                .Where(n => n.IsDeleted == false);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.NewsBlogStatus.StatusName == status);
            }

            var news = query
                .OrderByDescending(n => n.CreatedOn)
                .ToList();

            ViewBag.Status = status;

            return View(news);
        }
            
        [AuthorizeRole("User")]
        public IActionResult MyNews(string status)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;

            var query = _context.TblNewsBlogs
                .Include(n => n.NewsBlogStatus)
                .Include(n => n.Category)
                .Where(n => n.CreatedByUserId == userId && n.IsDeleted == false);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.NewsBlogStatus.StatusName == status);
            }

            var news = query
                .OrderByDescending(n => n.CreatedOn)
                .ToList();

            ViewBag.Status = status;

            return View("Index", news);
        }
       
        // CREATE – GET
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(
                _context.TblNewsCategories.Where(c => c.IsDeleted == false),
                "CategoryId",
                "CategoryName"
            );

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsBlogViewModel model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.TblNewsCategories.Where(c => c.IsDeleted == false),
                    "CategoryId",
                    "CategoryName"
                );
                return View(model);
            }
            if (ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Only JPG and PNG allowed");
                    ViewBag.Categories = new SelectList(
                        _context.TblNewsCategories.Where(c => c.IsDeleted == false),
                        "CategoryId",
                        "CategoryName"
                    );
                    return View(model);
                }
            }
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string role = HttpContext.Session.GetString("Role");

            // Status logic
            string statusName = (role == "Admin" || role == "SuperAdmin")
                                ? "Approved"
                                : "Pending";

            int statusId = _context.TblNewsBlogStatuses
                .Where(s => s.StatusName == statusName)
                .Select(s => s.NewsBlogStatusId)
                .First();

            var blog = new TblNewsBlog
            {
                CategoryId = model.CategoryId,
                Title = model.Title,
                Content = model.Content,
                CreatedByUserId = userId,
                CreatedOn = DateTime.Now,
                NewsBlogStatusId = statusId,
                IsDeleted = false,
                ApprovedByUserId = (role != "User") ? userId : null,
                ApprovedOn = (role != "User") ? DateTime.Now : null
            };

            _context.TblNewsBlogs.Add(blog);
            await _context.SaveChangesAsync();

            if (ImageFile != null)
            {
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                // Generate filename using NewsBlogId
                string newFileName = $"news_{blog.NewsBlogId}{extension}";

                string filePath = Path.Combine(uploadsFolder, newFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // Save image details in DB
                //_context.Update(blog);
                blog.ImagePath = "/uploads/" + newFileName;
                blog.OriginalFileName = ImageFile.FileName;
                await _context.SaveChangesAsync();

            }

            if (role == "User")
                return RedirectToAction("MyNews");
            else
                return RedirectToAction("Index");
        }

        // EDIT – GET
        public IActionResult Edit(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string role = HttpContext.Session.GetString("Role");

            TblNewsBlog? blog;

            if (role == "Admin" || role == "SuperAdmin")
            {
                blog = _context.TblNewsBlogs
                    .Include(u => u.NewsBlogStatus)
                    .FirstOrDefault(b => b.NewsBlogId == id && b.IsDeleted == false);
            }
            else
            {
                blog = _context.TblNewsBlogs
                    .Include(u => u.NewsBlogStatus)
                    .FirstOrDefault(b =>
                        b.NewsBlogId == id &&
                        b.CreatedByUserId == userId &&
                        b.IsDeleted == false);
            }


            if (blog == null)
                return RedirectToAction("UnauthorizedAccess", "Account");
            if (blog.NewsBlogStatus.StatusName != "Pending" &&
                role == "User")
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }

            var model = new NewsBlogViewModel
            {
                NewsBlogId = blog.NewsBlogId,
                CategoryId = blog.CategoryId,
                Title = blog.Title,
                Content = blog.Content,
                ImagePath=blog.ImagePath,
                OriginalFileName=blog.OriginalFileName
            };
            ViewBag.Categories = new SelectList(
            _context.TblNewsCategories.Where(c => c.IsDeleted == false),
            "CategoryId",
            "CategoryName",
            blog.CategoryId
        );

            return View(model);
        }
        // EDIT – POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewsBlogViewModel model,IFormFile ImageFile, bool RemoveImage)
        {
            if (ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Only JPG and PNG allowed");
                    ViewBag.Categories = new SelectList(
                        _context.TblNewsCategories.Where(c => c.IsDeleted == false),
                        "CategoryId",
                        "CategoryName",
                        model.CategoryId
                    );
                    return View(model);
                }
            }
            var userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("Role");

            TblNewsBlog? blog;

            if (role == "Admin" || role == "SuperAdmin")
            {
                blog = _context.TblNewsBlogs
                    .Include(u => u.NewsBlogStatus)
                    .FirstOrDefault(b => b.NewsBlogId == model.NewsBlogId && b.IsDeleted == false);

            }
            else
            {
                blog = _context.TblNewsBlogs
                    .Include(u => u.NewsBlogStatus)
                    .FirstOrDefault(b =>
                        b.NewsBlogId == model.NewsBlogId &&
                        b.CreatedByUserId == userId &&
                        b.IsDeleted == false);
            }
           
            
            if (blog == null)
                return RedirectToAction("UnauthorizedAccess", "Account");
            if (role == "User" && blog.NewsBlogStatus.StatusName != "Pending")
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }
            blog.Title = model.Title;
            blog.Content = model.Content;
            blog.CategoryId = model.CategoryId;
            blog.ModifiedOn = DateTime.Now;
            if (RemoveImage && !string.IsNullOrEmpty(blog.ImagePath))
            {
                string oldPath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    blog.ImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                blog.ImagePath = null;
                blog.OriginalFileName = null;
            }

            if (ImageFile != null)
            {
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // DELETE OLD IMAGE
                if (!string.IsNullOrEmpty(blog.ImagePath))
                {
                    string oldFilePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        blog.ImagePath.TrimStart('/')
                         );

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                string newFileName = $"news_{blog.NewsBlogId}{extension}";
                //string newFileName = $"news_{blog.NewsBlogId}_{DateTime.Now.Ticks}{extension}";
                string filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                blog.ImagePath = "/uploads/" + newFileName;
                blog.OriginalFileName = ImageFile.FileName;
            }

            await _context.SaveChangesAsync();

           // _context.SaveChanges();
            if (role=="User")
                return RedirectToAction("MyNews");
            else
                return RedirectToAction("Index");
        }
        // DELETE (Soft Delete)
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            TblNewsBlog? blog;

            if (role == "Admin" || role == "SuperAdmin")
            {
                blog = _context.TblNewsBlogs
                    .FirstOrDefault(b => b.NewsBlogId == id && b.IsDeleted == false);
            }
            else
            {
                blog = _context.TblNewsBlogs
                    .FirstOrDefault(b =>
                        b.NewsBlogId == id &&
                        b.CreatedByUserId == userId &&
                        b.IsDeleted == false);
            }

            if (blog == null)
                return RedirectToAction("UnauthorizedAccess", "Account");

            if (role == "User" && blog.NewsBlogStatus.StatusName != "Pending")
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }
            if (!string.IsNullOrEmpty(blog.ImagePath))
            {
                string filePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    blog.ImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }


            blog.IsDeleted = true;
            await _context.SaveChangesAsync();

            if (role == "User")
                return RedirectToAction("MyNews");
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeRole("Admin", "SuperAdmin")]
        public IActionResult ApproveReject(int id, string actionType)
        {
            var blog = _context.TblNewsBlogs.FirstOrDefault(b => b.NewsBlogId == id);
            if (blog == null) return NotFound();

            int adminId = HttpContext.Session.GetInt32("UserId").Value;

            if (actionType == "Approve")
            {
                blog.NewsBlogStatusId = _context.TblNewsBlogStatuses
                    .Where(s => s.StatusName == "Approved")
                    .Select(s => s.NewsBlogStatusId)
                    .First();
            }
            else if (actionType == "Reject")
            {
                blog.NewsBlogStatusId = _context.TblNewsBlogStatuses
                    .Where(s => s.StatusName == "Rejected")
                    .Select(s => s.NewsBlogStatusId)
                    .First();
            }

            blog.ApprovedByUserId = adminId;
            blog.ApprovedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
