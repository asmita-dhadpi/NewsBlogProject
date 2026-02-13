using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsBlogProject.Data;
using NewsBlogProject.Filters;
using NewsBlogProject.Models;
using NewsBlogProject.ViewModels;

namespace NewsBlogProject.Controllers
{
    [AuthorizeRole("Admin", "SuperAdmin")]
    public class NewsCategoryController : Controller
    {
        private readonly NewsBlogDbContext _context;

        public NewsCategoryController(NewsBlogDbContext context)
        {
            _context = context;
        }
        // LIST 
        public IActionResult Index()
        {
            var Categories = _context.TblNewsCategories
                .Where(b => b.IsDeleted == false)
                .OrderByDescending(b => b.CreatedOn)
                .ToList();

            return View(Categories);
        }
        // CREATE - GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - POST
        [HttpPost]
        public IActionResult Create(TblNewsCategory model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedOn = DateTime.Now;
            model.IsDeleted = false;

            _context.TblNewsCategories.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // EDIT - GET
        public IActionResult Edit(int id)
        {
            var category = _context.TblNewsCategories
                .FirstOrDefault(c => c.CategoryId == id && c.IsDeleted == false);

            if (category == null)
                return RedirectToAction("UnauthorizedAccess", "Account");
            //return View("~/Views/Shared/Unauthorized.cshtml");

            return View(category);
        }

        // EDIT - POST
        [HttpPost]
        public IActionResult Edit(TblNewsCategory model)
        {
            var category = _context.TblNewsCategories
                .FirstOrDefault(c => c.CategoryId == model.CategoryId && c.IsDeleted == false);

            if (category == null)
                return RedirectToAction("UnauthorizedAccess", "Account");
            //return View("~/Views/Shared/Unauthorized.cshtml");

            category.CategoryName = model.CategoryName;
            category.ModifiedOn = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        // DELETE (SOFT)
        public IActionResult Delete(int id)
        {
            var category = _context.TblNewsCategories
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return RedirectToAction("UnauthorizedAccess", "Account");
           // return View("~/Views/Shared/Unauthorized.cshtml");

            category.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
