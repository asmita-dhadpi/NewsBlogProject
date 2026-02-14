using NewsBlogProject.Models;
using System.ComponentModel.DataAnnotations;

namespace NewsBlogProject.ViewModels
{
    public class NewsBlogViewModel
    {
        public int NewsBlogId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
    public class BlogDetailsViewModel
    {
        public TblNewsBlog Blog { get; set; }

        public List<TblComment> Comments { get; set; }

        public string CommentText { get; set; }
    }

}
