namespace NewsBlogProject.ViewModels
{
    public class SuperAdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int ApprovedNews { get; set; }
        public int RejectedNews { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalNews { get; set; }
        public string Filter { get; set; }
    }
}
