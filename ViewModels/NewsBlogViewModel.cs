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
}
