namespace NewsBlogProject.ViewModels
{
    public class UserDashboardVM
    {
        public int MyTotalNews { get; set; }
        public int MyApprovedNews { get; set; }
        public int MyRejectedNews { get; set; }
        public string Filter { get; set; }
    }
}
