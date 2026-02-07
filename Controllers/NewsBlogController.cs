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

        public NewsBlogController(NewsBlogDbContext context)
        {
            _context = context;
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
        public IActionResult Create(NewsBlogViewModel model)
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
            _context.SaveChanges();

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
                Content = blog.Content
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
        public IActionResult Edit(NewsBlogViewModel model)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string role = HttpContext.Session.GetString("Role");

            TblNewsBlog? blog;

            if (role == "Admin" || role == "SuperAdmin")
            {
                blog = _context.TblNewsBlogs
                    .FirstOrDefault(b => b.NewsBlogId == model.NewsBlogId && b.IsDeleted == false);
            }
            else
            {
                blog = _context.TblNewsBlogs
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

            _context.SaveChanges();
            if (role=="User")
                return RedirectToAction("MyNews");
            else
                return RedirectToAction("Index");
        }
        // DELETE (Soft Delete)
        public IActionResult Delete(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
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

            blog.IsDeleted = true;
            _context.SaveChanges();

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
