using System.ComponentModel.DataAnnotations;

namespace NewsBlogProject.ViewModels
{
    public class CommentViewModel
    {
        public int NewsBlogId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string CommentText { get; set; }
    }
}
