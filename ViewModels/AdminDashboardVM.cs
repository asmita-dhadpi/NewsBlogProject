namespace NewsBlogProject.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int ApprovedNews { get; set; }
        public int RejectedNews { get; set; }
        public string Filter { get; set; }
    }
}
