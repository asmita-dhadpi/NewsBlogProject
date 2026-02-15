using Microsoft.AspNetCore.Mvc;
using NewsBlogProject.Data;
using NewsBlogProject.Filters;
using NewsBlogProject.Models;
using NewsBlogProject.ViewModels;

namespace NewsBlogProject.Controllers
{
    [AuthorizeRole("User", "Admin", "SuperAdmin")]
    public class CommentsController : Controller
    {
        private readonly NewsBlogDbContext _context;

        public CommentsController(NewsBlogDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Add(CommentViewModel model, int id)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Home", new { id = model.NewsBlogId });

            int userId = HttpContext.Session.GetInt32("UserId").Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            //  CHECK BLOG EXISTS
            var blogExists = _context.TblNewsBlogs
                .Any(b => b.NewsBlogId == id && b.IsDeleted == false);

            if (!blogExists)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.CommentText))
            {
                return RedirectToAction("Details", new { model.NewsBlogId });
            }
            var comment = new TblComment
            {
                NewsBlogId = id,
                UserId = userId,
                CommentText = model.CommentText,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _context.TblComments.Add(comment);
            _context.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = id });
        }

        public IActionResult Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            var comment = _context.TblComments
                .FirstOrDefault(c => c.CommentId == id);

            if (comment == null)
                return NotFound();

            if (userId == null ||
               (comment.UserId != userId &&
                role != "Admin" &&
                role != "SuperAdmin"))
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }

            return View(comment);
        }
        [HttpPost]
        public IActionResult Edit(TblComment model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            var comment = _context.TblComments
                .FirstOrDefault(c => c.CommentId == model.CommentId);

            if (comment == null)
                return NotFound();

            if (userId == null ||
               (comment.UserId != userId &&
                role != "Admin" &&
                role != "SuperAdmin"))
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }

            comment.CommentText = model.CommentText;
            _context.SaveChanges();

            return RedirectToAction("Details", "Home",
                new { id = comment.NewsBlogId });
        }
        public IActionResult Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            var comment = _context.TblComments
                .FirstOrDefault(c => c.CommentId == id);

            if (comment == null)
                return NotFound();

            if (userId == null ||
               (comment.UserId != userId &&
                role != "Admin" &&
                role != "SuperAdmin"))
            {
                return RedirectToAction("UnauthorizedAccess", "Account");
            }

            //_context.TblComments.Remove(comment);
            comment.IsActive= false;
            _context.SaveChanges();

            return RedirectToAction("Details", "Home",
                new { id = comment.NewsBlogId });
        }
        // Admin: Enable / Disable comment
        [AuthorizeRole("Admin", "SuperAdmin")]
        public IActionResult ToggleStatus(int id)
        {
            var comment = _context.TblComments.FirstOrDefault(c => c.CommentId == id);
            if (comment == null)
                return NotFound();

            comment.IsActive = !comment.IsActive;
            _context.SaveChanges();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
