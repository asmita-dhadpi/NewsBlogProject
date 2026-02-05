using NewsBlogProject.Models;

namespace NewsBlogProject.ViewModels
{
    public class HomePageViewModel
    {
        // Sidebar
        public List<TblNewsCategory> Categories { get; set; }

        // Blog list
        public List<TblNewsBlog> Blogs { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);

        // Filters
        public int? CategoryId { get; set; }
        public string Search { get; set; }
    }
}
