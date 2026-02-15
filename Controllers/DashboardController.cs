using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewsBlogProject.Data;
using NewsBlogProject.Filters;
using NewsBlogProject.ViewModels;
using System;

namespace NewsBlogProject.Controllers
{
    [AuthorizeRole("SuperAdmin", "Admin", "User")]
    public class DashboardController : Controller
    {
        private readonly string _connectionString;

        public DashboardController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        [AuthorizeRole("SuperAdmin", "Admin", "User")]
        // ENTRY POINT (after login)
        public IActionResult Index(string filter = "All")
        {
            string role = HttpContext.Session.GetString("Role");

            if (role == "SuperAdmin")
                return View("SuperAdminDashboard", GetSuperAdminData(filter));

            if (role == "Admin")
                return View("AdminDashboard", GetAdminData(filter));

            return View("UserDashboard", GetUserData(filter));
        }
        // ================== HELPERS ==================

        private string GetDateCondition(string filter)
        {
            if (filter == "Today")
                return " AND CAST(b.CreatedOn AS DATE) = CAST(GETDATE() AS DATE) ";

            if (filter == "Month")
                return " AND MONTH(b.CreatedOn) = MONTH(GETDATE()) AND YEAR(b.CreatedOn) = YEAR(GETDATE()) ";

            return "";
        }
        // ================== SUPER ADMIN ==================

        private SuperAdminDashboardVM GetSuperAdminData(string filter)
        {
            var model = new SuperAdminDashboardVM();
            string dateCondition = GetDateCondition(filter);

            using SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand($@"
            SELECT
              (SELECT COUNT(*) FROM tblUsers WHERE IsDeleted = 0),
              (SELECT COUNT(*) FROM tblUsers u
                JOIN tblRoles r ON u.RoleId = r.RoleId
                WHERE r.RoleName = 'Admin' AND u.IsDeleted = 0),
              (SELECT COUNT(*) FROM tblNewsBlog b WHERE b.IsDeleted = 0 {dateCondition}),
              (SELECT COUNT(*) FROM tblNewsBlog b
                JOIN tblNewsBlogStatus s ON b.NewsBlogStatusId = s.NewsBlogStatusId
                WHERE s.StatusName = 'Approved' AND b.IsDeleted = 0 {dateCondition}),
              (SELECT COUNT(*) FROM tblNewsBlog b
                JOIN tblNewsBlogStatus s ON b.NewsBlogStatusId = s.NewsBlogStatusId
                WHERE s.StatusName = 'Rejected' AND b.IsDeleted = 0  {dateCondition})
        ", con); 
            
            con.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                model.TotalUsers = r.GetInt32(0);
                model.TotalAdmins = r.GetInt32(1);
                model.TotalNews = r.GetInt32(2);
                model.ApprovedNews = r.GetInt32(3);
                model.RejectedNews = r.GetInt32(4);
            }

            model.Filter = filter;
            return model;
        }

        // ================== ADMIN ==================

        private AdminDashboardVM GetAdminData(string filter)
        {
            var model = new AdminDashboardVM();
            string dateCondition = GetDateCondition(filter);

            using SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand($@"
            SELECT
              (SELECT COUNT(*) FROM tblUsers WHERE IsDeleted = 0 AND RoleId=3),
              (SELECT COUNT(*) FROM tblNewsBlog b
                JOIN tblNewsBlogStatus s ON b.NewsBlogStatusId = s.NewsBlogStatusId
                WHERE s.StatusName = 'Approved' AND b.IsDeleted = 0  {dateCondition}),
              (SELECT COUNT(*) FROM tblNewsBlog b
                JOIN tblNewsBlogStatus s ON b.NewsBlogStatusId = s.NewsBlogStatusId
                WHERE s.StatusName = 'Rejected' AND b.IsDeleted = 0  {dateCondition})
        ", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                model.TotalUsers = r.GetInt32(0);
                model.ApprovedNews = r.GetInt32(1);
                model.RejectedNews = r.GetInt32(2);
            }

            model.Filter = filter;
            return model;
        }

        // ================== USER ==================

        private UserDashboardVM GetUserData(string filter)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var model = new UserDashboardVM();
            string dateCondition = GetDateCondition(filter);

            using SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand($@"
                SELECT
                  COUNT(*) AS TotalNews,
                  ISNULL(SUM(CASE WHEN s.StatusName = 'Approved' THEN 1 ELSE 0 END), 0) AS ApprovedNews,
                  ISNULL(SUM(CASE WHEN s.StatusName = 'Rejected' THEN 1 ELSE 0 END), 0) AS RejectedNews
                FROM tblNewsBlog b
                JOIN tblNewsBlogStatus s ON b.NewsBlogStatusId = s.NewsBlogStatusId
                WHERE b.CreatedByUserId = @UserId
                  AND b.IsDeleted = 0
                  {dateCondition}
            ", con);

            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                model.MyTotalNews = r.GetInt32(0);
                model.MyApprovedNews = r.GetInt32(1);
                model.MyRejectedNews = r.GetInt32(2);
            }

            model.Filter = filter;
            return model;
        }
       

    }
}
