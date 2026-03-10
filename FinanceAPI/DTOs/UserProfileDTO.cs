using FinanceAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class UserProfileDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "FirstName is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
