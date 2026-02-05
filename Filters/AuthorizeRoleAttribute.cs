using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewsBlogProject.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var userId = httpContext.Session.GetInt32("UserId");
            var role = httpContext.Session.GetString("Role");

            // Not logged in
            if (userId == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null
                );
                return;
            }

            // Role not allowed
            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Account",
                    null
                );
            }
        }
    }
}
