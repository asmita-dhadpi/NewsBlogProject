using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlogProject.Data;
using NewsBlogProject.Models;
using NewsBlogProject.ViewModels;
using System.Diagnostics;

namespace NewsBlogProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly NewsBlogDbContext _context;

        public HomeController(NewsBlogDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
        int? categoryId,
        string search,
        int page = 1)
            {
                int pageSize = 5;

                var approvedStatusId = _context.TblNewsBlogStatuses
                    .Where(s => s.StatusName == "Approved")
                    .Select(s => s.NewsBlogStatusId)
                    .First();

                // Base query
                var query = _context.TblNewsBlogs
                    .Include(b => b.Category)
                    .Where(b => b.NewsBlogStatusId == approvedStatusId &&
                                b.IsDeleted == false);

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(b =>
                        b.Title.Contains(search) ||
                        b.Content.Contains(search));
                }

                // Category filter
                if (categoryId.HasValue)
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }

                int totalRecords = query.Count();

                var blogs = query
                    .OrderByDescending(b => b.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new HomePageViewModel
                {
                    Categories = _context.TblNewsCategories
                        .Where(c => c.IsDeleted == false)
                        .ToList(),

                    Blogs = blogs,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    CategoryId = categoryId,
                    Search = search
                };
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BlogList", model);
            }
            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
