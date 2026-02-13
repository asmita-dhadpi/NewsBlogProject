using System.ComponentModel.DataAnnotations;

namespace NewsBlogProject.ViewModels
{
    public class UserViewModel
    {
        public int? UserId { get; set; }
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        public string? PhoneNumber { get; set; }

        public int RoleId { get; set; }
    }
}
