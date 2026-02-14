using System.ComponentModel.DataAnnotations;

namespace NewsBlogProject.ViewModels
{
    public class UserViewModel
    {
        public int? UserId { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }
    }
}
